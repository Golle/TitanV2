namespace Titan.Systems;
/// <summary>
/// 
/// </summary>
/// <param name="Stage">At what engine stage should the system be run</param>
/// <param name="ExecutionType">The type of execution</param>
/// <param name="Order">
/// The order the system will be sorted in.
/// <para>
/// Lower value means it will be executed before other systems, higher value means it will be executed after.<br/>
/// Order only affects systems that have same dependencies.<br/>
/// There's no guarantee that the order will work, it all depends on the dependencies that the system has.
/// </para>
/// </param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SystemAttribute(SystemStage Stage = SystemStage.Update, SystemExecutionType ExecutionType = SystemExecutionType.Normal, int Order = 0) : Attribute;
