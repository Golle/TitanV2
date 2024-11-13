using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Titan.Platform.Win32.GDI;
using static System.Net.Mime.MediaTypeNames;

namespace Titan.Platform.Win32;

public static unsafe partial class Gdi32
{
    private const string DllName = "Gdi32";

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial uint GetFontData(
        HDC hdc,
        uint dwTable,
        uint dwOffset,
        void* pvBuffer,
        uint cjBuffer
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int DrawText(
        HDC hdc,
        byte* lpchText,
        int cchText,
        RECT* lprc,
        uint format
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HDC CreateCompatibleDC(
        HDC hdc
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HBITMAP CreateCompatibleBitmap(
        HDC hdc,
        int cx,
        int cy
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTextMetricsW(
        HDC hdc,
        TEXTMETRICW* lptm
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTextMetricsA(
        HDC hdc,
        TEXTMETRICA* lptm
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TextOutA(
        HDC hdc,
        int x,
        int y,
        byte* lpString,
        int c
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool TextOutW(
        HDC hdc,
        int x,
        int y,
        char* lpString,
        int c
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HGDIOBJ SelectObject(
        HDC hdc,
        HGDIOBJ h
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int AddFontResourceExW(
        char* name,
        FontResource fl,
        void* res
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int AddFontResourceExA(
        byte* name,
        FontResource fl,
        void* res
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HFONT CreateFontA(
        int cHeight,
        int cWidth,
        int cEscapement,
        int cOrientation,
        FontWeight cWeight,
        uint bItalic,
        uint bUnderline,
        uint bStrikeOut,
        Charset iCharSet,
        InOutPrecision iOutPrecision,
        ClipPrecision iClipPrecision,
        Quality iQuality,
        PitchAndFamily iPitchAndFamily,
        byte* pszFaceName
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HFONT CreateFontW(
        int cHeight,
        int cWidth,
        int cEscapement,
        int cOrientation,
        FontWeight cWeight,
        uint bItalic,
        uint bUnderline,
        uint bStrikeOut,
        Charset iCharSet,
        InOutPrecision iOutPrecision,
        ClipPrecision iClipPrecision,
        Quality iQuality,
        PitchAndFamily iPitchAndFamily,
        char* pszFaceName
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial COLORREF SetBkColor(
        HDC hdc,
        COLORREF color
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial COLORREF SetTextColor(
        HDC hdc,
        COLORREF color
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int SetBkMode(
        HDC hdc,
        BkMode mode
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int EnumFontFamiliesA(
        HDC hdc,
        byte* lpLogfont,
        delegate* unmanaged<LOGFONTA*, TEXTMETRICA*, uint, void*, int> lpProc,
        void* lParam
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int EnumFontFamiliesW(
        HDC hdc,
        char* lpLogfont,
        delegate* unmanaged<LOGFONTW*, TEXTMETRICW*, uint, void*, int> lpProc,
        void* lParam
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial HBITMAP CreateDIBSection(
        HDC hdc,
        BITMAPINFO* pbmi,
        DibColorIdentifiers usage,
        void** ppvBits,
        HANDLE hSection,
        uint offset
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTextExtentPoint32A(
        HDC hdc,
        byte* lpString,
        int c,
        SIZE* psizl
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTextExtentPoint32W(
        HDC hdc,
        char* lpString,
        int c,
        SIZE* psizl
    );


    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetTextExtentExPointW(
        HDC hdc,
        char* lpszString,
        int cchString,
        int nMaxExtent,
        int* lpnFit,
        int* lpnDx,
        SIZE* lpSize
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int SetMapMode(
        HDC hdc,
        MappingMode iMode
    );

    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCharABCWidthsW(
        HDC hdc,
        uint wFirst,
        uint wLast,
        ABC* lpABC
    );


    [LibraryImport(DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial int GetDIBits(
        HDC hdc,
        HBITMAP hbm,
        uint start,
        uint cLines,
        void* lpvBits,
        BITMAPINFO* lpbmi,
        DibColorIdentifiers usage
    );
}
