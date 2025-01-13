using Titan.Core.Threading;

namespace Titan.Assets;

internal unsafe struct Asset
{
    public AssetState State;
    public AssetFile* File;
    public AssetSystem* System;
    public AssetRegistry* Registry;
    public AssetDependency* Dependencies;
    public byte NumberOfDependencies;

    public JobHandle AsyncJobHandle;

    public void* FileBuffer;
    public void* Resource;

#if HOT_RELOAD_ASSETS
    public uint FileSize;
#endif

    public AssetDescriptor* Descriptor;
    public ReadOnlySpan<AssetDependency> GetDependencies() => new(Dependencies, NumberOfDependencies);
    public AssetLoaderDescriptor* GetLoader() => System->Loaders.GetPointer((int)Descriptor->Type);
}
