using Titan.Audio.Events;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.Core.Memory.Allocators;
using Titan.Events;
using Titan.Platform.Win32;
using Titan.Platform.Win32.MMAPI;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Audio.CoreAudio;

internal readonly struct AudioDeviceInfo(TitanArray<char> id, TitanArray<char> name)
{
    public ReadOnlySpan<char> Id => id.AsReadOnlySpan();
    public ReadOnlySpan<char> Name => name.AsReadOnlySpan();
    public unsafe char* GetIdAsPointer() => id.AsPointer();
}

[UnmanagedResource]
internal unsafe partial struct CoreAudioSystem
{
    private BumpAllocator Allocator;
    private TitanList<AudioDeviceInfo> AudioDevices;
    private AudioDeviceInfo* DefaultDevice;

    public readonly ReadOnlySpan<AudioDeviceInfo> GetDevices() => AudioDevices.AsReadOnlySpan();
    public readonly AudioDeviceInfo* GetDefaultDevice() => DefaultDevice;
    public readonly AudioDeviceInfo* FindDeviceByID(ReadOnlySpan<char> id)
    {
        if (id.IsEmpty)
        {
            return null;
        }

        for (var i = 0; i < AudioDevices.Count; ++i)
        {
            var device = AudioDevices.GetPointer(i);
            if (device->Id.Equals(id, StringComparison.InvariantCulture))
            {
                return device;
            }
        }

        return null;
    }

    [System(SystemStage.PreInit)]
    public static void PreInit(CoreAudioSystem* audio, IMemoryManager memoryManager)
    {
        if (!memoryManager.TryCreateBumpAllocator(out audio->Allocator, MemoryUtils.KiloBytes(256)))
        {
            Logger.Error<CoreAudioSystem>("Failed to create the allocator.");
            return;
        }

        audio->RefreshDevices();
    }


    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(CoreAudioSystem* audioSystem, EventReader<AudioDeviceChangedEvent> audioDeviceChanged)
    {
        if (!audioDeviceChanged.HasEvents)
        {
            return;
        }

        //NOTE(Jens): Workaround for HasEvents issue
        foreach (ref readonly var _ in audioDeviceChanged)
        {
            audioSystem->RefreshDevices();
        }
    }

    [System(SystemStage.PostShutdown)]
    public static void PostShutdown(CoreAudioSystem* system, IMemoryManager memoryManager)
    {
        system->AudioDevices = default;
        system->DefaultDevice = null;
        memoryManager.FreeAllocator(system->Allocator);
    }

    private void RefreshDevices()
    {
        using var _ = new MeasureTime<CoreAudioSystem>("Completed Audio Device refresh in {0} ms");

        AudioDevices = default;
        DefaultDevice = null;
        Allocator.Reset();

        using ComPtr<IMMDeviceEnumerator> enumerator = default;
        var hr = Ole32.CoCreateInstance(CLSID.CLSID_MMDeviceEnumerator, null, CLSCTX.CLSCTX_ALL, enumerator.UUID, (void**)enumerator.GetAddressOf());

        if (FAILED(hr))
        {
            Logger.Error<CoreAudioSystem>($"Failed to Create the {nameof(IMMDeviceEnumerator)}. HRESULT = {hr}");
            return;
        }

        using ComPtr<IMMDeviceCollection> devices = default;
        hr = enumerator.Get()->EnumAudioEndpoints(EDataFlow.eRender, DeviceState.DEVICE_STATE_ACTIVE, devices.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<CoreAudioSystem>($"Failed to enumerate the Audio Devices. HRESULT = {hr}");
            return;
        }

        uint count;
        hr = devices.Get()->GetCount(&count);
        if (FAILED(hr))
        {
            Logger.Error<CoreAudioSystem>($"Failed to get the count of the Audio Devices. HRESULT = {hr}");
            return;
        }

        AudioDevices = Allocator.AllocateList<AudioDeviceInfo>(count);

        for (var i = 0u; i < count; ++i)
        {
            //var deviceInfo = AudioDevices.GetPointer(i);
            using ComPtr<IMMDevice> device = default;
            hr = devices.Get()->Item(i, device.GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<CoreAudioSystem>($"Failed to get the device at index {i}. HRESULT = {hr}");
                continue;
            }

            // Read the ID
            char* deviceId;
            hr = device.Get()->GetId(&deviceId);
            if (FAILED(hr))
            {
                Logger.Error<CoreAudioSystem>($"Failed to get the device ID. HRESULT = {hr}");
                continue;
            }

            var deviceIdLength = MSVCRT.wcslen(deviceId);
            var id = Allocator.AllocateArray<char>(deviceIdLength);
            MemoryUtils.Copy(id, deviceId, deviceIdLength);
            Ole32.CoTaskMemFree(deviceId);


            // Read the name of the Device
            using ComPtr<IPropertyStore> properties = default;
            hr = device.Get()->OpenPropertyStore(StorageAccessMode.STGM_READ, properties.GetAddressOf());
            if (FAILED(hr))
            {
                Logger.Error<CoreAudioSystem>($"Failed to get the property store of Audio Device at index {i}. HRESULT = {hr}");
                continue;
            }

            // PKEY_Device_FriendlyName: {A45C254E-DF1C-4EFD-8020-67D146A850E0}, PID = 14
            PROPERTYKEY FriendlyDeviceNameKey = new()
            {
                fmtid = new Guid(0xA45C254E, 0xDF1C, 0x4EFD, 0x80, 0x20, 0x67, 0xD1, 0x46, 0xA8, 0x50, 0xE0),
                pid = 14
            };

            // Get the friendly name property value
            PROPVARIANT friendlyName;
            hr = properties.Get()->GetValue(&FriendlyDeviceNameKey, &friendlyName);
            if (FAILED(hr))
            {
                Logger.Error<CoreAudioSystem>($"Failed to get the friendly name of Audio Device at index {i}. HRESULT = {hr}");
                continue;
            }

            //NOTE(Jens): We add 1 to include the null terminator. We need it when we create XAudio mastering voice.
            var length = MSVCRT.wcslen((char*)friendlyName.p);
            var name = Allocator.AllocateArray<char>(length);
            MemoryUtils.Copy(name, (char*)friendlyName.p, length);
            Ole32.PropVariantClear(&friendlyName);

            AudioDevices.Add(new(id, name));
        }

        // Read and set the Default Device.
        using ComPtr<IMMDevice> defaultDevice = default;
        hr = enumerator.Get()->GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, defaultDevice.GetAddressOf());
        if (FAILED(hr))
        {
            //NOTE(Jens): This might happen if no audio device is plugged in. Maybe we should be fine with this.
            Logger.Warning<CoreAudioSystem>($"Failed to get the Default Audio Endpoint. HRESULT = {hr}");
            DefaultDevice = null;
        }
        else
        {
            char* defaultDeviceIdPtr;
            hr = defaultDevice.Get()->GetId(&defaultDeviceIdPtr);
            if (FAILED(hr))
            {
                Logger.Error<CoreAudioSystem>("Failed to get the ID of the the Default Audio.");
                return;
            }
            //var defaultDeviceId = new TitanArray<char>(defaultDeviceIdPtr, (uint)MSVCRT.wcslen(defaultDeviceIdPtr) + 1);
            var deviceId = new ReadOnlySpan<char>(defaultDeviceIdPtr, MSVCRT.wcslen(defaultDeviceIdPtr));
            for (var i = 0; i < AudioDevices.Count; ++i)
            {
                var deviceInfo = AudioDevices.GetPointer(i);

                if (deviceInfo->Id.Equals(deviceId, StringComparison.InvariantCulture))
                {
                    Logger.Trace<CoreAudioSystem>($"Found the default device. Name={deviceInfo->Name}");
                    DefaultDevice = deviceInfo;
                }
            }
            Ole32.CoTaskMemFree(defaultDeviceIdPtr);
        }
    }

}
