using System.Runtime.InteropServices;
using Titan.Assets.Types;
using Titan.Core.Memory;
using Titan.Platform.Win32;
using Titan.Platform.Win32.GDI;
using Titan.Tools.AssetProcessor.Metadata.Types;
using Titan.UI.Text;

namespace Titan.Tools.AssetProcessor.Processors.Fonts;

internal sealed class FontProcessor : AssetProcessor<FontMetadata>
{
    protected override async Task OnProcess(FontMetadata metadata, IAssetDescriptorContext context)
        => await Task.Run(() => OnProcessInternal(metadata, context));

    private unsafe void OnProcessInternal(FontMetadata metadata, IAssetDescriptorContext context)
    {
        var typeface = metadata.Typeface ?? Path.GetFileNameWithoutExtension(metadata.ContentFileFullPath);
        var hdc = Gdi32.CreateCompatibleDC(default);
        var characterCount = metadata.Characters.Length;
        var characters = metadata.Characters.Order().ToArray();

        var fontSize = 50;

        fixed (char* pFile = metadata.ContentFileFullPath)
        fixed (char* pTypeFace = typeface)
        {
            Gdi32.AddFontResourceExW(pFile, FontResource.FR_PRIVATE, null);

            var font = Gdi32.CreateFontW(fontSize, 00, 0, 0,
                FontWeight.FW_NORMAL,
                0, 0, 0,
                Charset.DEFAULT_CHARSET,
                InOutPrecision.OUT_DEFAULT_PRECIS,
                ClipPrecision.CLIP_DEFAULT_PRECIS,
                Quality.CLEARTYPE_QUALITY,
                PitchAndFamily.DEFAULT_PITCH | PitchAndFamily.FF_DONTCARE,
                pTypeFace
            );
            Gdi32.SelectObject(hdc, font);
        }

        var padding = 4;
        SIZE textSize;
        fixed (char* ptr = characters)
        {
            if (!Gdi32.GetTextExtentPoint32W(hdc, ptr, characterCount, &textSize))
            {
                context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to get the text extent. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
                return;
            }
        }

        var max = metadata.Characters.Max(c => (int)c);
        var min = (char)metadata.Characters.Min(c => (int)c);

        //NOTE(Jens): We align the size of the sprite with 256, this is a requirement for resources in D3D12.
        var width = MemoryUtils.AlignToUpper((uint)(textSize.X + padding * (characterCount - 1)), 256u);
        var height = fontSize;

        BITMAPINFO info = default;
        info.bmiHeader = new()
        {
            biBitCount = 32,
            biClrImportant = 0,
            biClrUsed = 0,
            biCompression = BICompression.BI_RGB,
            biHeight = height,
            biWidth = (int)width,
            biPlanes = 1,
            biSize = (uint)sizeof(BITMAPINFOHEADER),
            biSizeImage = (uint)(width * height * 4),
            biXPelsPerMeter = 0,
            biYPelsPerMeter = 0
        };

        void* bits;
        var hBitmap = Gdi32.CreateDIBSection(hdc, &info, DibColorIdentifiers.DIB_RGB_COLORS, &bits, default, 0);
        Gdi32.SelectObject(hdc, hBitmap);

        Gdi32.SetBkMode(hdc, BkMode.TRANSPARENT);
        //Gdi32.SetBkColor(hdc, COLORREF.FromRGB(0, 0, 0));   // Black background
        Gdi32.SetTextColor(hdc, COLORREF.FromRGB(255, 255, 255)); // White text


        var defaultGlyphIndex = -1;

        Span<GlyphInfo> glyphs = stackalloc GlyphInfo[characterCount];
        var xOffset = 0;
        for (var i = 0; i < characterCount; ++i)
        {
            var c = characters[i];
            if (c == metadata.DefaultGlyph)
            {
                defaultGlyphIndex = i;
            }
            // Draw character
            Gdi32.TextOutW(hdc, xOffset, 0, &c, 1);

            // Get the size of the character
            SIZE charSize;
            Gdi32.GetTextExtentPoint32W(hdc, &c, 1, &charSize);

            glyphs[i] = new()
            {
                Character = (byte)c,
                Y = 0,
                X = (ushort)xOffset,
                Height = (byte)charSize.Y,
                Width = (byte)charSize.X,
            };

            // Move the offset for the next character
            xOffset += charSize.X + padding;
        }

        if (defaultGlyphIndex == -1)
        {
            context.AddDiagnostics(DiagnosticsLevel.Warning, $"The default glyph {metadata.DefaultGlyph} does not exist in the text. Using first glyph as default. {glyphs[0].Character}");
            defaultGlyphIndex = 0;
        }

        var pixeldata = new ReadOnlySpan<byte>(bits, (int)info.bmiHeader.biSizeImage);
        var descriptor = new FontDescriptor
        {
            DefaultGlyphIndex = (ushort)defaultGlyphIndex,
            NumberOfGlyphs = checked((ushort)characterCount),
            BytesPerPixel = 4,
            Height = (ushort)height,
            Width = (ushort)width
        };
        if (!context.TryAddFont(descriptor, glyphs, pixeldata, metadata))
        {
            context.AddDiagnostics(DiagnosticsLevel.Error, $"Failed to add the Font to the context. Id = {metadata.Id}. Name = {metadata.Name}. Path = {metadata.ContentFileRelativePath}");
        }

        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //{
        //    //NOTE(Jens): this is not needed, we only care about the raw bytes.
        //    var woop = Image.FromHbitmap(hBitmap);
        //    var filename = @$"c:\tmp\bitmap{metadata.Id.ToString()[..6]}-rolf.bmp";
        //    if (File.Exists(filename))
        //    {
        //        File.Delete(filename);
        //    }
        //    woop.Save(filename);
        //}
    }


    [UnmanagedCallersOnly]
    private static unsafe int EnumerateFonts(LOGFONTW* font, TEXTMETRICW* metric, uint dword, void* param)
    {
        var str = new string(font->lfFaceName, 0, LOGFONTW.LF_FACESIZE);
        Console.WriteLine($"Font: {str}");
        return 1;
    }
}

