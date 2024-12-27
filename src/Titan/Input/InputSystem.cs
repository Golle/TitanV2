using System.Numerics;
using Titan.Core.Logging;
using Titan.Core.Maths;
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
    public static void Update(InputState* state, in Window window, EventReader<KeyUpEvent> keyUpEvents, EventReader<KeyDownEvent> keyDownEvents, EventReader<CharacterTypedEvent> characterEvents, EventReader<WindowLostFocusEvent> lostFocusEvents, EventReader<WindowGainedFocusEvent> gainedFocusEvents)
    {
        var lostFocus = lostFocusEvents.Any();
        if (lostFocus)
        {
            Logger.Trace<InputSystem>("Window lost focus, clearing key states");
            MemoryUtils.Init(state->Current, KeyStateSize);
            MemoryUtils.Init(state->MouseState, MouseStateSize);
        }

        var isWindowInFocus = window.IsFocused();

        MemoryUtils.Copy(state->Previous, state->Current, KeyStateSize);
        MemoryUtils.Copy(state->PreviousMouseState, state->MouseState, MouseStateSize);

        // If we hide the cursor we want to put it back where it was.
        if (window.CursorVisible && state->MouseHidden)
        {
            // restore state
            window.SetCursorPosition(state->MousePositionSavedState);
        }
        else if (!window.CursorVisible && state->MouseVisible)
        {
            // The frame where the mouse gets hidden we store the state and set the cursor to the center of the screen.
            state->MousePositionSavedState = window.GetAbsoluteCursorPosition();
            window.SetCursorPosition(new Point(window.ScreenWidth / 2, window.ScreenHeight / 2));
            // save state
        }

        if (isWindowInFocus)
        {
            state->MouseState[(int)MouseButton.Left] = window.IsButtonDown(MouseButton.Left);
            state->MouseState[(int)MouseButton.Right] = window.IsButtonDown(MouseButton.Right);
            state->MouseState[(int)MouseButton.Middle] = window.IsButtonDown(MouseButton.Middle);
            state->MouseState[(int)MouseButton.XButton1] = window.IsButtonDown(MouseButton.XButton1);
            state->MouseState[(int)MouseButton.XButton2] = window.IsButtonDown(MouseButton.XButton2);
        }
        

        state->MouseHidden = !window.CursorVisible;


        if (state->MouseVisible)
        {
            var mousePosition = window.GetRelativeCursorPosition();
            state->PreviousMousePosition = state->MousePosition;
            state->MousePosition = mousePosition;
            state->PreviousMousePositionUI = state->MousePositionUI;
            state->MousePositionUI = mousePosition with { Y = window.Height - mousePosition.Y };
            state->MousePositionDelta = (Vector2)(state->MousePosition - state->PreviousMousePosition);
        }
        else
        {
            // When the cursor is hidden, we use the absolute position.
            var mousePosition = window.GetAbsoluteCursorPosition();
            state->MousePositionUI = new Point(-1, -1);
            // always put the mouse in the center of the screen.
            state->PreviousMousePosition = state->MousePosition = new(window.ScreenWidth / 2, window.ScreenHeight / 2);
            // calculate the delta in the movement.
            state->MousePositionDelta = (Vector2)(state->MousePosition - mousePosition);
        }

        if (!window.CursorVisible)
        {
            window.SetCursorPosition(new Point(window.ScreenWidth / 2, window.ScreenHeight / 2));
        }

        state->CharactersTyped = 0;
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
    }
}
