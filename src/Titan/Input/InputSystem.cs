using System.Numerics;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Systems;
using Titan.Windows.Win32;

namespace Titan.Input;

internal unsafe partial struct InputSystem
{
    private const uint KeyStateSize = sizeof(bool) * (uint)KeyCode.NumberOfKeys;

    [System]
    public static void Update(ref InputState state, IWindow window)
    {
        fixed (bool* pCurrent = state.Current)
        fixed (bool* pPrevious = state.Previous)
        {
            MemoryUtils.Copy(pPrevious, pCurrent, KeyStateSize);
        }

        var mousePosition = window.GetRelativeCursorPosition();

        state.PreviousMousePos = state.MousePos;
        state.MousePos = mousePosition;
        state.MouseDelta1 = (Vector2)(state.MousePos - state.PreviousMousePos);
        state.OutsideWindow = mousePosition.Y < 0 || mousePosition.X < 0 || mousePosition.X > window.Width || mousePosition.Y > window.Height;
    }

}
