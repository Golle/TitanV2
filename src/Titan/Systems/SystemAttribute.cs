namespace Titan.Systems;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SystemAttribute(SystemStage stage = SystemStage.Update) : Attribute;
