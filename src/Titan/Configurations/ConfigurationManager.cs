using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Titan.Core.IO;
using Titan.Core.Logging;
using Titan.Core.Memory;
using Titan.IO.FileSystem;

namespace Titan.Configurations;

public interface IConfigurationManager : IService
{
    /// <summary>
    /// Returns the Configuration of type T or the Default value returned by the Default function.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <returns>The configuration</returns>
    T GetConfigOrDefault<T>() where T : IConfiguration, IDefault<T>;

    /// <summary>
    /// Updates the config, if it's a persistable config it will write it to disk when the game shuts down.
    /// </summary>
    /// <typeparam name="T">The configuration type</typeparam>
    /// <param name="config">The patches configuration</param>
    void UpdateConfig<T>(T config) where T : IConfiguration;
}

internal sealed class ConfigurationManager : IConfigurationManager
{
    private readonly Dictionary<Type, (ConfigurationDescriptor Descriptor, IConfiguration Config)> _configs = new();
    private IFileSystem? _fileSystem;
    public bool Init(IFileSystem fileSystem, IReadOnlyList<ConfigurationDescriptor> configs)
    {
        Logger.Info<ConfigurationManager>($"Reading configs. Count = {configs.Count}");
        foreach (var descriptor in configs)
        {
            Logger.Trace<ConfigurationManager>($"Config Type = {descriptor.Config.GetType().Name} FileName = {descriptor.Filename} JsonTypeInfo = {descriptor.TypeInfo != null}");

            var config = ReadConfigOrDefault(fileSystem, descriptor);

            ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_configs, descriptor.Type, out _);
            entry.Config = config;
            entry.Descriptor = descriptor;
        }

        _fileSystem = fileSystem;
        return true;
    }

    private static IConfiguration ReadConfigOrDefault(IFileSystem fileSystem, in ConfigurationDescriptor descriptor)
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
        Logger.Info<ConfigurationManager>("Persisting configurations");

        foreach (var (descriptor, config) in _configs.Values)
        {
            WriteConfig(descriptor, config);
        }
    }

    private void WriteConfig(in ConfigurationDescriptor descriptor, IConfiguration configuration)
    {
        Debug.Assert(_fileSystem != null);
        if (string.IsNullOrWhiteSpace(descriptor.Filename) || descriptor.TypeInfo == null)
        {
            return;
        }

        var fileHandle = _fileSystem.Open(descriptor.Filename, FilePathType.Configs, true);
        if (fileHandle.IsInvalid())
        {
            Logger.Error<ConfigurationManager>($"Can't open the config file {descriptor.Filename}. Type = {configuration.GetType()}");
            return;
        }

        //NOTE(Jens): We should probably write to a temp file and then copy it instead of writing directly to the file.
        _fileSystem.Truncate(fileHandle);

        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, configuration, descriptor.TypeInfo);
        var bytes = stream.ToArray();
        var bytesWritten = _fileSystem.Write(fileHandle, bytes);
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
        => (T)(_configs.TryGetValue(typeof(T), out var config) ? config.Config : T.Default);

    public void UpdateConfig<T>(T config) where T : IConfiguration
    {
        ref var configEntry = ref CollectionsMarshal.GetValueRefOrNullRef(_configs, typeof(T));
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
