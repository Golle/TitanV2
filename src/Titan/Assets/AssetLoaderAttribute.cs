namespace Titan.Assets;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class AssetLoaderAttribute<T> : Attribute where T : IAsset;
