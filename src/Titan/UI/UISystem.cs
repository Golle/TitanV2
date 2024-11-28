using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Configurations;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Maths;
using Titan.Core.Memory;
using Titan.Graphics.D3D12;
using Titan.Input;
using Titan.Rendering;
using Titan.Resources;
using Titan.Systems;
using static Titan.Platform.Win32.Win32Common;

namespace Titan.UI;

[StructLayout(LayoutKind.Sequential)]
internal struct UIElement
{
    public Color Color;
    public SizeF Size;
    public Vector2 Offset;
    public TextureCoordinate TextureCoordinates;
    public int TextureId;
    public UIElementType Type;
    public float Repeat;
    private uint Padding;
    //public uint GlyphIndex;
}

internal enum UIElementType
{
    None = 0,
    Sprite = 1,
    Text = 2,
    SpriteRepeat = 3
}

internal struct UIState
{
    public int NextId;
    public int ActiveId;
    public int HighlightedId;
}

[UnmanagedResource]
internal unsafe partial struct UISystem
{
    private const int InvalidId = -1;

    public Handle<Rendering.Buffer> Instances;
    public Handle<Rendering.Buffer> GlyphInstances;
    public uint Count;
    private UIElement* ElementsGPU;

    private TitanArray<UIElement> ElementsCPU;
    private UIState State;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsActive(int id)
        => State.ActiveId == id;

    /// <summary>
    /// Set the ID to the active element
    /// </summary>
    /// <param name="id">The ID of the UI element</param>
    /// <returns>True if this is the active one, false if some other UI element was already active.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SetActive(int id)
    {
        if (IsActive(id))
        {
            return true;
        }

        return Interlocked.CompareExchange(ref State.ActiveId, id, InvalidId) == InvalidId;
    }

    public bool SetHighlighted(int id)
    {
        if (State.HighlightedId == id)
        {
            return true;
        }
        return Interlocked.CompareExchange(ref State.HighlightedId, id, InvalidId) == InvalidId;
    }

    [System(SystemStage.Init)]
    public static void Init(UISystem* uiSystem, in D3D12ResourceManager resourceManager, IMemoryManager memoryManager, IConfigurationManager configurationManager)
    {
        var config = configurationManager.GetConfigOrDefault<UIConfig>();
        var s = sizeof(UIElement) * config.MaxElements;
        // Allocate memory for CPU side
        if (!memoryManager.TryAllocArray(out uiSystem->ElementsCPU, config.MaxElements))
        {
            Logger.Error<UISystem>($"Failed to allocate memory for UIElements. Count = {config.MaxElements} Size = {sizeof(UIElement) * config.MaxElements}");
            return;
        }

        // Set up GPU resources
        uiSystem->Instances = resourceManager.CreateBuffer(CreateBufferArgs.Create<UIElement>(1024, BufferType.Structured, cpuVisible: true, shaderVisible: true));
        if (uiSystem->Instances.IsInvalid)
        {
            Logger.Error<UISystem>("Failed to create a structured buffer for UI Elements.");
            return;
        }

        ////NOTE(Jens): Maybe this should be configurable? 
        //const uint glyphcount = 10 * 256;
        //uiSystem->GlyphInstances = resourceManager.CreateBuffer(CreateBufferArgs.Create<Glyph>(glyphcount, BufferType.Structured, cpuVisible: true, shaderVisible: true));
        //if (uiSystem->GlyphInstances.IsInvalid)
        //{
        //    Logger.Error<UISystem>("Failed to create a structured buffer for Glyphs.");
        //    return;
        //}

        // map the resources
        var hr = resourceManager.Access(uiSystem->Instances)->Resource.Get()->Map(0, null, (void**)&uiSystem->ElementsGPU);
        if (FAILED(hr))
        {
            Logger.Error<UISystem>("Failed to Map the UI elements buffer.");
            return;
        }

        //hr = resourceManager.Access(uiSystem->GlyphInstances)->Resource.Get()->Map(0, null, (void**)&uiSystem->GlyphsGPU);
        //if (FAILED(hr))
        //{
        //    Logger.Error<UISystem>("Failed to Map the Glyph instances buffer.");
        //    return;
        //}
    }

    [System(SystemStage.PreUpdate)]
    public static void PreUpdate(ref UISystem system)
    {
        system.Count = 0;
        system.State.NextId = 1;
        system.State.HighlightedId = InvalidId;
        //system.State.ActiveId = -1;
    }

    [System(SystemStage.PostUpdate, SystemExecutionType.Inline)]
    public static void Update(ref UISystem system, in InputState inputState)
    {
        if (inputState.IsButtonUp(MouseButton.Left))
        {
            system.State.ActiveId = InvalidId;
        }
        //else if (system.State.ActiveId == 0)
        //{
        //    system.State.ActiveId = InvalidId;
        //}

        MemoryUtils.Copy(system.ElementsGPU, system.ElementsCPU.AsPointer(), (uint)sizeof(UIElement) * system.Count);
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(UISystem* stuff, in D3D12ResourceManager resourceManager)
    {
        if (stuff->Instances.IsValid)
        {
            Logger.Warning<UISystem>("cant shut down this..");
            //resourceManager.DestroyBuffer(stuff->Instances);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in UIElement element)
    {
        var index = Interlocked.Increment(ref Count) - 1;
        ElementsCPU[index] = element;
    }

    public void Add(ReadOnlySpan<UIElement> elements)
    {
        if (elements.Length == 0)
        {
            return;
        }
        var length = (uint)elements.Length;
        var index = Interlocked.Add(ref Count, length) - length;
        Debug.Assert(index + length < ElementsCPU.Length);
        MemoryUtils.Copy(ElementsCPU.GetPointer(index), elements);
    }

}
