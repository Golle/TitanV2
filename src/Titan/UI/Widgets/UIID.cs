using System.Runtime.CompilerServices;
#pragma warning disable CS0660, CS0661

namespace Titan.UI.Widgets;

public readonly struct UIID
{
    public readonly int Id;
    private static int _next;
    private UIID(int id) => Id = id;
    public static UIID Create()
        => new(Interlocked.Increment(ref _next));

    public static void Create(Span<UIID> ids)
    {
        foreach (ref var id in ids)
        {
            id = Create();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in UIID lh, in UIID rh) => rh.Id == lh.Id;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in UIID lh, in UIID rh) => lh.Id != rh.Id;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(in UIID id) => id.Id;
}
