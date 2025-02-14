using System.Diagnostics;
using Titan.Application;
using Titan.Assets;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Input;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;
using Titan.UI.Resources;

namespace Titan.UI2;

internal struct UIState
{
    public const int InvalidId = 0;
    public int ActiveId;
    public int FocusId;

    /// <summary>
    /// The ID for the highligthed last frame
    /// </summary>
    public int HighlightedId;
    public Inline10<(int Id, int Layer)> Highligthed;
    public int HighlightedCount;

    public Point CursorPosition;
    public bool ButtonPressed;
    public bool ButtonDown;
    public bool ButtonReleased;
    public int GetHighligthedId()
    {
        var id = InvalidId;
        var layer = int.MinValue;
        for (var i = 0; i < HighlightedCount; ++i)
        {
            ref readonly var hot = ref Highligthed[i];
            if (hot.Layer > layer)
            {
                id = hot.Id;
                layer = hot.Layer;
            }
        }

        return id;
    }

    public bool SetActive(int id)
    {
        Logger.Trace<UIState>($"Active Widget: {id} (Highligthed ID: {HighlightedId}) Is match: {id == HighlightedId}");
        if (HighlightedId == id)
        {
            ActiveId = id;
            return true;
        }

        return false;
    }
    /// <summary>
    /// Sets the widget to have Keyboard focus.
    /// </summary>
    /// <param name="id">Id of the widget</param>
    public void SetFocus(int id)
    {
        Logger.Trace<UIState>($"Focus Widget: {id}");
        FocusId = id;
    }

    public void ClearFocus(int id)
    {
        Logger.Trace<UIState>($"Clear Focus Widget: {id}");
        Interlocked.CompareExchange(ref FocusId, 0, id);
    }

    /// <summary>
    /// Adds an element to the highlight list, it will be sorted before next frame.
    /// Multiple elements can be added at the same time, but the one with highest value for layer will be used.
    /// </summary>
    /// <param name="id">Id of the widget</param>
    /// <param name="layer">The layer for the context</param>
    public void SetHighlighted(int id, int layer)
    {
        //Logger.Trace<UIState>($"Highlight Widget: {id} Layer = {layer}");
        var index = Interlocked.Increment(ref HighlightedCount);
        Highligthed[index - 1] = (id, layer);
    }
}


[UnmanagedResource]
internal unsafe partial struct UISystem2
{
    public UIStyle DefaultStyle;
    public UIState State;

    private TitanArray<UIWidget> Widgets;
    private int Count;

    private Inline2<MappedGPUResource<UIWidget>> WidgetBuffers;

    public readonly Handle<GPUBuffer> GetCurrentGPUHandle() => WidgetBuffers[EngineState.FrameIndex].Handle;
    public readonly uint GetCount() => (uint)Count;


    private static readonly Comparison<UIWidget> WidgetComparison = (x, y) => x.Id.CompareTo(y.Id);

    [System(SystemStage.Init)]
    public static void Init(ref UISystem2 system, IMemoryManager memoryManager, in D3D12ResourceManager resourceManager, AssetsManager assetsManager)
    {
        const uint MaxUIElements = 10 * 1024;
        memoryManager.TryAllocArray(out system.Widgets, MaxUIElements);

        // Set up GPU resources
        for (var i = 0; i < 2; ++i)
        {
            var args = CreateBufferArgs.Create<UIWidget>(MaxUIElements, BufferType.Structured, cpuVisible: true, shaderVisible: true);
            var buffer = resourceManager.CreateBuffer(args);
            resourceManager.MapBuffer(out system.WidgetBuffers[i], buffer);
        }

        var defaultSprite = assetsManager.Load<SpriteAsset>(EngineAssetsRegistry.Sprites.DebugUiStyle.Asset);
        var defaultFont = assetsManager.Load<FontAsset>(EngineAssetsRegistry.Fonts.RobotoMonoRegular);
        system.DefaultStyle = new()
        {
            Button =
            {
                Asset = defaultSprite,
                ButtonIndexStart = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Button02,
                ButtonSelectedIndexStart = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Button02Selected,
                ButtonDownIndexStart = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Button02Pressed,
                IsNinePatch = true
            },
            Slider =
            {
                Asset = defaultSprite,
                BackgroundIndexLeft = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderLeft,
                BackgroundIndexCenter = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderCenter,
                BackgroundIndexRight = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderRight,
                BackgroundIndexEmptyCenter = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyCenter,
                BackgroundIndexEmptyLeft = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyLeft,
                BackgroundIndexEmptyRight = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderEmptyRight,
                SliderIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderBlob,
                SliderSize = new(32),
                SliderSelectedIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SliderBlobSelected
            },
            Checkbox =
            {
                Asset = defaultSprite,
                Index = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.Checkbox,
                SelectedIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.CheckboxSelected,
                CheckmarkIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.CheckboxCheckmark
            },
            Textbox =
            {
              Asset = defaultSprite,
              Font = defaultFont,
              CursorIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.TextBoxCursor,
              FocusIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.TextBoxFocus,
              Index = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.TextBox
            },
            SelectBox =
            {
                Asset = defaultSprite,
                Font = defaultFont,
                Index = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SelectBox,
                FocusIndex = EngineAssetsRegistry.Sprites.DebugUiStyle.SpriteIndex.SelectBoxFocus,
                ItemMargin = 2
            },
            Font =
            {
                Asset = defaultFont
            }
        };
    }

    public void Submit(ReadOnlySpan<UIWidget> widgets)
    {
        var count = widgets.Length;
        var offset = Interlocked.Add(ref Count, count) - count;

        Debug.Assert(offset + count < Widgets.Length);
        widgets.CopyTo(Widgets.AsSpan()[offset..]);
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(ref UISystem2 system, in InputState inputState)
    {
        if (inputState.MouseHidden)
        {
            // if the mouse is hidden we set the position to -1 and remove any highlights
            system.State.CursorPosition = new(-1, -1);
            system.State.HighlightedId = UIState.InvalidId;
            system.State.ActiveId = UIState.InvalidId;
            system.State.FocusId = UIState.InvalidId;
            system.State.ButtonDown = false;
            system.State.ButtonReleased = false;
            system.State.ButtonPressed = false;
        }
        else
        {
            system.State.CursorPosition = inputState.MousePositionUI;
            system.State.ButtonDown = inputState.IsButtonDown(MouseButton.Left);
            system.State.ButtonReleased = inputState.IsButtonReleased(MouseButton.Left);
            system.State.ButtonPressed = inputState.IsButtonPressed(MouseButton.Left);
        }

        system.Count = 0;
        system.State.HighlightedId = system.State.GetHighligthedId();
        system.State.HighlightedCount = 0;

    }

    [System(SystemStage.PostUpdate)]
    public static void PostUpdate(ref UISystem2 system)
    {
        if (system.State.ButtonReleased)
        {
            system.State.ActiveId = UIState.InvalidId;
        }

        if (system.Count == 0)
        {
            return;
        }

        //NOTE(Jens): This causes overdraw, since we'll be drawing elements that are in the back first.
        //TODO(Jens): Measure the impact of the overdraw, we could introduce a depth buffer( or something similar) to prevent it, but it will most likely not be so bad for the small amount of UI elements.
        var widgets = system.Widgets.AsSpan()[..system.Count];
        widgets.Sort(WidgetComparison);
        system.WidgetBuffers[EngineState.FrameIndex].Write(widgets);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(ref UISystem2 system, in D3D12ResourceManager resourceManager)
    {
        for (var i = 0; i < 2; ++i)
        {
            resourceManager.Unmap(system.WidgetBuffers[i]);
            resourceManager.DestroyBuffer(system.WidgetBuffers[i].Handle);
        }
        system.WidgetBuffers = default;
    }


}

