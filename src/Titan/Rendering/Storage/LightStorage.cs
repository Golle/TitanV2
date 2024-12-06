using System.Numerics;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.ECS.Components;
using Titan.Graphics.D3D12;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.Rendering.Storage;



[StructLayout(LayoutKind.Sequential, Size = 48)]
internal struct GPULight
{
    public Vector3 Position;
    public Vector3 Direction;
    public ColorRGB Color;
    public float IntensityOrRadius;
    // We've got 8 bytes extra here to use.
}


[UnmanagedResource]
internal unsafe partial struct LightStorage
{
    public Handle<GPUBuffer> LightStorageHandle;

    private TitanArray<GPULight> _cpuLights;
    private GPULight* _gpuLights;

    //TODO(Jens): For now there's no way to destroy a light. We'll just keep increasing until we crash.
    private short _lightCount;

    [System(SystemStage.Init)]
    public static void Init(LightStorage* storage, in D3D12ResourceManager resourceManager, IConfigurationManager configurationManager, IMemoryManager memoryManager)
    {
        var conf = configurationManager.GetConfigOrDefault<D3D12Config>();
        var maxLights = conf.Resources.MaxLights;

        storage->LightStorageHandle = resourceManager.CreateBuffer(CreateBufferArgs.Create<GPULight>(maxLights, BufferType.Structured, cpuVisible: true, shaderVisible: true));

        if (storage->LightStorageHandle.IsInvalid)
        {
            Logger.Error<LightStorage>($"Failed to create the LighstStorage buffer. Count = {maxLights}");
            return;
        }

        if (FAILED(resourceManager.Access(storage->LightStorageHandle)->Resource.Get()->Map(0, null, (void**)&storage->_gpuLights)))
        {
            Logger.Error<LightStorage>("Failed to map the lights buffer.");
            return;
        }

        if (!memoryManager.TryAllocArray(out storage->_cpuLights, maxLights))
        {
            Logger.Error<LightStorage>($"Failed to alloc the CPU array for lights. Count = {maxLights} Size = {sizeof(GPULight) * maxLights} bytes");
            return;
        }
    }

    [System(SystemStage.PreUpdate)]
    public static void Update(LightStorage* storage, Span<Light> lights, ReadOnlySpan<Transform3D> transforms)
    {
        for (var i = 0; i < lights.Length; ++i)
        {
            ref var light = ref lights[i];
            if (light.LightIndex == 0)
            {
                // we start at 1
                light.LightIndex = ++storage->_lightCount;
            }

            ref var cpuLight = ref storage->_cpuLights[light.LightIndex];
            cpuLight.Position = transforms[i].Position;
            cpuLight.Direction = light.Direction;
            cpuLight.IntensityOrRadius = light.Intensity;
            cpuLight.Color = light.Color;

            //TODO(Jens): Add check fort "dirty" lights.  For now we update everything every frame.
        }

        // update all in a single copy. We should measure this and see what's more effiecent. We could use a Dirty flag on the light.
        MemoryUtils.Copy(storage->_gpuLights, storage->_cpuLights.AsPointer(), sizeof(GPULight) * (storage->_lightCount + 1));
    }
}
