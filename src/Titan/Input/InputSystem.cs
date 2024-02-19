using System.Numerics;
using Titan.Core.Memory;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Input;

internal unsafe partial struct InputSystem
{
    private const uint KeyStateSize = sizeof(bool) * (uint)KeyCode.NumberOfKeys;

    [System(SystemStage.PreUpdate)]
    public static void Update(InputState* state, in Window window)
    {
        MemoryUtils.Copy(state->Previous, state->Current, KeyStateSize);

        var mousePosition = window.GetRelativeCursorPosition();

        state->PreviousMousePosition = state->MousePosition;
        state->MousePosition = mousePosition;
        state->MousePositionDelta = (Vector2)(state->MousePosition - state->PreviousMousePosition);
    }
}
