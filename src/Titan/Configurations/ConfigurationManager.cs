using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.IO.FileSystem;

namespace Titan.Configurations;

internal sealed class ConfigurationManager(IFileSystem fileSystem) : IConfigurationManager
{
    private readonly Dictionary<Type, (ConfigurationDescriptor Descriptor, IConfiguration Config)> _configurations = new();
    public bool Init(ImmutableArray<ConfigurationDescriptor> configurations)
    {
        Logger.Info<ConfigurationManager>("Reading configurations!");
        foreach (ref readonly var descriptor in configurations.AsSpan())
        {
            Logger.Trace<ConfigurationManager>($"Config Type = {descriptor.Config.GetType().Name} FileName = {descriptor.Filename} JsonTypeInfo = {descriptor.TypeInfo != null}");
            ref var configEntry = ref CollectionsMarshal.GetValueRefOrAddDefault(_configurations, descriptor.Config.GetType(), out _);
            configEntry.Descriptor = descriptor;
            configEntry.Config = ReadConfigOrDefault(descriptor);
        }

        return true;
    }


    private IConfiguration ReadConfigOrDefault(in ConfigurationDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Filename) || descriptor.TypeInfo == null)
        {
            return descriptor.Config;
        }

        var fileHandle = fileSystem.Open(descriptor.Filename, FilePathType.Configs);
        if (fileHandle.IsInvalid())
        {
            Logger.Trace<ConfigurationManager>($"No configuration file found for {descriptor.Config.GetType().Name}. Using default.");
            return descriptor.Config;
        }

        var length = fileSystem.GetLength(fileHandle);
        Debug.Assert(length <= MemoryUtils.KiloBytes(100), "Configuration file that's over 100KB, implement support for bigger buffers.");
        Span<byte> buffer = stackalloc byte[(int)length];
        var bytesRead = fileSystem.Read(fileHandle, buffer);
        Debug.Assert(bytesRead == length);
        fileSystem.Close(ref fileHandle);

        //NOTE(Jens): Add error handling
        try
        {
            return (IConfiguration)JsonSerializer.Deserialize(buffer, descriptor.TypeInfo!)!;
        }
        catch (Exception e)
        {
            Logger.Error<ConfigurationManager>($"Failed to deserialize config file. Using Default, Filename = {descriptor.Filename}. Exception = {e.GetType().Name} Message = {e.Message}");
            return descriptor.Config;
        }
    }
    public void Shutdown()
    {
        Logger.Info<IConfigurationManager>("Persisting configurations");

        foreach (var configEntry in _configurations.Values)
        {
            WriteConfig(configEntry.Descriptor, configEntry.Config);
        }

    }

    private void WriteConfig(in ConfigurationDescriptor descriptor, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Filename) || descriptor.TypeInfo == null)
        {
            return;
        }

        var fileHandle = fileSystem.Open(descriptor.Filename, FilePathType.Configs, true);
        if (fileHandle.IsInvalid())
        {
            Logger.Error<ConfigurationManager>($"Can't open the config file {descriptor.Filename}. Type = {configuration.GetType()}");
            return;
        }
        fileSystem.Truncate(fileHandle);

        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, configuration, descriptor.TypeInfo);
        var bytes = stream.ToArray();
        var bytesWritten = fileSystem.Write(fileHandle, bytes);
        if (bytesWritten == -1)
        {
            Logger.Error<ConfigurationManager>("Failed to write the configuration file.");
        }
        if (bytesWritten != bytes.Length)
        {
            Logger.Warning<ConfigurationManager>($"The number of bytes written is different from buffer length. Buffer = {bytes.Length} Bytes Written = {bytesWritten} Filename = {descriptor.Filename}");
        }
    }

    public T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>
        => (T)(_configurations.TryGetValue(typeof(T), out var a) ? a.Config : T.Default);

    public void UpdateConfig<T>(T config) where T : IConfiguration
    {
        ref var configEntry = ref CollectionsMarshal.GetValueRefOrNullRef(_configurations, typeof(T));
        if (!Unsafe.IsNullRef(ref configEntry))
        {
            configEntry.Config = config;
        }
        else
        {
            Logger.Warning<ConfigurationManager>($"Trying to update an config that hasn't been added. Type = {typeof(T).Name}");
        }
    }
}
