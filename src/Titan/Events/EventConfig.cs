namespace Titan.Events;

internal record EventConfig(uint MaxEventTypes, uint MaxEventsPerFrame, uint MaxEventSize) : IConfiguration, IDefault<EventConfig>
{
    public const uint DefaultMaxEvntTypes = 1024;
    public const uint DefaultMaxEventsPerFrame = 1024;
    
    public const uint DefaultMaxEventSize = 32;
    public static EventConfig Default => new(DefaultMaxEvntTypes, DefaultMaxEventsPerFrame, DefaultMaxEventSize);
}
