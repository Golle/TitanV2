namespace Titan.Events;

/// <summary>
/// Marker attribute for Events, this will generated a unique ID for the event.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class EventAttribute : Attribute;
