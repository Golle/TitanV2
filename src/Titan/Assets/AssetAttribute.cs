namespace Titan.Assets;

[AttributeUsage(AttributeTargets.Struct)]
public sealed class AssetAttribute(AssetType Type) : Attribute;
