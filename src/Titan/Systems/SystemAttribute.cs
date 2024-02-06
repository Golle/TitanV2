namespace Titan.Systems;

[AttributeUsage(AttributeTargets.Method)]
public sealed class SystemAttribute(SystemStage Stage = SystemStage.Update) : Attribute;
