using System.Numerics;
using Titan.Core.Maths;
using Titan.Resources;

namespace Titan.Input;

[UnmanagedResource]
public unsafe partial struct InputState
{
    public fixed bool Current[(int)KeyCode.NumberOfKeys];
    public fixed bool Previous[(int)KeyCode.NumberOfKeys];
    public Point MousePosition;
    public Point PreviousMousePosition;
    public Vector2 MousePositionDelta;
    public bool OutsideWindow;

    public readonly bool IsKeyDown(KeyCode code) => Current[(int)code];
    public readonly bool IsKeyUp(KeyCode code) => !Current[(int)code];
    public readonly bool IsKeyPressed(KeyCode code) => IsKeyDown(code) && !Previous[(int)code];
    public readonly bool IsKeyReleased(KeyCode code) => IsKeyUp(code) && Previous[(int)code];
}

