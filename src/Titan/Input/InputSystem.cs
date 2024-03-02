using System.Numerics;
using Titan.Core.Memory;
using Titan.Events;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Input;

internal unsafe partial struct InputSystem
{
    private const uint KeyStateSize = sizeof(bool) * (uint)KeyCode.NumberOfKeys;

    [System(SystemStage.PreUpdate)]
    public static void Update(InputState* state, in Window window, EventReader<KeyUpEvent> keyUpEvents, EventReader<KeyDownEvent> keyDownEvents, EventReader<CharacterTypedEvent> characterEvents)
    {
        MemoryUtils.Copy(state->Previous, state->Current, KeyStateSize);

        var mousePosition = window.GetRelativeCursorPosition();

        state->CharactersTyped = 0;
        state->PreviousMousePosition = state->MousePosition;
        state->MousePosition = mousePosition;
        state->MousePositionDelta = (Vector2)(state->MousePosition - state->PreviousMousePosition);

        foreach (ref readonly var charater in characterEvents)
        {
            state->Characters[state->CharactersTyped++] = charater.Character;
        }

        foreach (ref readonly var keyDown in keyDownEvents)
        {
            state->Current[(int)keyDown.Code] = true;
        }

        foreach (ref readonly var keyUp in keyUpEvents)
        {
            state->Current[(int)keyUp.Code] = false;
        }

        //TODO(Jens): Implement lost focux, we need to clear the buffer in that case.
    }
}
