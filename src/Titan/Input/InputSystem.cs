using System.Numerics;
using Titan.Core.Memory;
using Titan.Systems;
using Titan.Windows.Win32;

namespace Titan.Input;

internal unsafe partial struct InputSystem
{
    private const uint KeyStateSize = sizeof(bool) * (uint)KeyCode.NumberOfKeys;

    [System(SystemStage.PreUpdate)]
    public static void Update(InputState* state, IWindow window)
    {
        MemoryUtils.Copy(state->Previous, state->Current, KeyStateSize);

        var mousePosition = window.GetRelativeCursorPosition();

        state->PreviousMousePosition = state->MousePosition;
        state->MousePosition = mousePosition;
        state->MousePositionDelta = (Vector2)(state->MousePosition - state->PreviousMousePosition);
        state->OutsideWindow = mousePosition.Y < 0 || mousePosition.X < 0 || mousePosition.X > window.Width || mousePosition.Y > window.Height;
    }
}
