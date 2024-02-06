using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Titan.Core.Maths;
using Titan.Resources;

namespace Titan.Input;

[UnmanagedResource]
internal unsafe partial struct InputState
{
    internal fixed bool Current[(int)KeyCode.NumberOfKeys];
    internal fixed bool Previous[(int)KeyCode.NumberOfKeys];

    internal Point MousePos;
    internal Point PreviousMousePos;
    internal Vector2 MouseDelta1;
    internal bool OutsideWindow;


    public readonly bool IsKeyDown(KeyCode code) => Current[(int)code];
    public readonly bool IsKeyUp(KeyCode code) => !Current[(int)code];
    public readonly bool IsKeyPressed(KeyCode code) => IsKeyDown(code) && !Previous[(int)code];
    public readonly bool IsKeyReleased(KeyCode code) => IsKeyUp(code) && Previous[(int)code];

    [UnscopedRef]
    public ref readonly Point MousePosition => ref MousePos;
    [UnscopedRef]
    public ref readonly Point PrevioisMousePosition => ref PreviousMousePos;
    [UnscopedRef]
    public ref readonly Vector2 MouseDelta => ref MouseDelta1;
    public bool MouseOutsideWindow => OutsideWindow;
}

