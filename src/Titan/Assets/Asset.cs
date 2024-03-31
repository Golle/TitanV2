using Titan.Core.Threading;

namespace Titan.Assets;

internal unsafe struct Asset
{
    public AssetState State;
    public AssetFile* File;
    public AssetsContext* Context;

    public JobHandle AsyncJobHandle;

    public void* FileBuffer;
    public void* Resource;
    
    public AssetDescriptor* Descriptor;


    public AssetLoaderDescriptor* GetLoader() => Context->Loaders.GetPointer((int)Descriptor->Type);
}
