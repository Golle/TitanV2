using System.Numerics;
using Titan.Core.Memory;
using Titan.Events;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Input;

internal unsafe partial struct InputSystem
{
    private const uint KeyStateSize = sizeof(bool) * (uint)KeyCode.NumberOfKeys;
    private const uint MouseStateSize = sizeof(bool) * (uint)MouseButton.Count;

    [System(SystemStage.PreUpdate)]
    public static void Update(InputState* state, in Window window, EventReader<KeyUpEvent> keyUpEvents, EventReader<KeyDownEvent> keyDownEvents, EventReader<CharacterTypedEvent> characterEvents)
    {
        MemoryUtils.Copy(state->Previous, state->Current, KeyStateSize);
        MemoryUtils.Copy(state->PreviousMouseState, state->MouseState, MouseStateSize);

        var mousePosition = window.GetRelativeCursorPosition();
        state->MouseState[(int)MouseButton.Left] = window.IsButtonDown(MouseButton.Left);
        state->MouseState[(int)MouseButton.Right] = window.IsButtonDown(MouseButton.Right);
        state->MouseState[(int)MouseButton.Middle] = window.IsButtonDown(MouseButton.Middle);
        state->MouseState[(int)MouseButton.XButton1] = window.IsButtonDown(MouseButton.XButton1);
        state->MouseState[(int)MouseButton.XButton2] = window.IsButtonDown(MouseButton.XButton2);

        state->CharactersTyped = 0;
        state->PreviousMousePosition = state->MousePosition;
        state->PreviousMousePositionUI = state->PreviousMousePositionUI;
        state->MousePosition = mousePosition;
        state->MousePositionUI = mousePosition with { Y = window.Height - mousePosition.Y };
        state->MousePositionDelta = (Vector2)(state->MousePosition - state->PreviousMousePosition);

        foreach (ref readonly var charater in characterEvents)
        {
            state->Characters[state->CharactersTyped++] = charater.Character;
        }

        //NOTE(Jens): We don't clear the Current state, bug?
        foreach (ref readonly var keyDown in keyDownEvents)
        {
            state->Current[(int)keyDown.Code] = true;
        }

        foreach (ref readonly var keyUp in keyUpEvents)
        {
            state->Current[(int)keyUp.Code] = false;
        }

        //TODO(Jens): Implement lost focus, we need to clear the buffer in that case.
    }
}
