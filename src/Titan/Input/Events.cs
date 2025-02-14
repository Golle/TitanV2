using Titan.Core.Maths;
using Titan.Events;

namespace Titan.Input;

[Event]
public partial record struct KeyDownEvent(KeyCode Code, bool Repeat);

[Event]
public partial record struct KeyUpEvent(KeyCode Code);

[Event]
public partial record struct CharacterTypedEvent(char Character);

[Event]
public partial record struct WindowLostFocusEvent;

[Event]
public partial record struct WindowGainedFocusEvent;

[Event]
public partial record struct WindowResizeEvent(uint Width, uint Height)
{
    public Size Size => new((int)Width, (int)Height);
    public SizeF SizeF => new(Width, Height);
}

[Event]
public partial record struct MouseWheelDeltaEvent(short Delta);
