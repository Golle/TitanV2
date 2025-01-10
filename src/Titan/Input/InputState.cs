using System.Numerics;
using Titan.Core.Maths;
using Titan.Resources;

namespace Titan.Input;



public enum MouseButton : byte
{
    //NOTE(Jens): these codes map to Win32 VKs.
    Left = 0x01,
    Right = 0x02,
    Middle = 0x04,
    XButton1 = 0x05,
    XButton2 = 0x06,
    Count
}

[UnmanagedResource]
public unsafe partial struct InputState
{
    public const int MaxTypedCharacters = 32;

    public fixed bool Current[(int)KeyCode.NumberOfKeys];
    public fixed bool Previous[(int)KeyCode.NumberOfKeys];

    //TODO(Jens): The mouse state does not support the logical mapping of buttons. This can be solved with GetSYstemMetrics(SM_SWAPBUTTON). 
    //TODO(Jens): We'll implement that when needed.
    public fixed bool MouseState[(int)MouseButton.Count];
    public fixed bool PreviousMouseState[(int)MouseButton.Count];

    public fixed char Characters[MaxTypedCharacters];
    public int CharactersTyped;

    public Point MousePosition;
    public Point PreviousMousePosition;
    public Point MousePositionUI;
    public Point PreviousMousePositionUI;
    public Vector2 MousePositionDelta;
    internal Point MousePositionSavedState;
    public int MouseWheelDelta;
    public bool OutsideWindow;
    public bool MouseVisible => !MouseHidden;
    public bool MouseHidden;

    public readonly bool IsKeyDown(KeyCode code) => Current[(int)code];
    public readonly bool IsKeyUp(KeyCode code) => !Current[(int)code];
    public readonly bool IsKeyPressed(KeyCode code) => IsKeyDown(code) && !Previous[(int)code];
    public readonly bool IsKeyReleased(KeyCode code) => IsKeyUp(code) && Previous[(int)code];

    public readonly bool IsButtonDown(MouseButton button) => MouseState[(int)button];
    public readonly bool IsButtonUp(MouseButton button) => !MouseState[(int)button];
    public readonly bool IsButtonPressed(MouseButton button) => IsButtonDown(button) && !PreviousMouseState[(int)button];
    public readonly bool IsButtonReleased(MouseButton button) => IsButtonUp(button) && PreviousMouseState[(int)button];

    public readonly ReadOnlySpan<char> GetCharacters()
    {
        if (CharactersTyped == 0)
        {
            return ReadOnlySpan<char>.Empty;
        }
        fixed (char* pCharacters = Characters)
        {
            return new(pCharacters, CharactersTyped);
        }
    }
}
