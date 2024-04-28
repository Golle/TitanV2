using System.Drawing;
using System.Runtime.InteropServices;
using Titan.Platform.Win32;
using Titan.Platform.Win32.GDI;
using Titan.Tools.AssetProcessor.Metadata.Types;

namespace Titan.Tools.AssetProcessor.Processors.Fonts;

internal sealed class FontProcessor : AssetProcessor<FontMetadata>
{
    protected override unsafe Task OnProcess(FontMetadata metadata, IAssetDescriptorContext context)
    {
        var typeface = metadata.Typeface ?? Path.GetFileNameWithoutExtension(metadata.ContentFileFullPath);

        var deviceContext = Gdi32.CreateCompatibleDC(default);

        TEXTMETRICA textA;
        TEXTMETRICW textW;

        var resultA = Gdi32.GetTextMetricsA(deviceContext, &textA);
        var resultW = Gdi32.GetTextMetricsW(deviceContext, &textW);


        fixed (char* pFile = metadata.ContentFileFullPath)
        fixed (char* pTypeFace = typeface)
        {
            Gdi32.AddFontResourceExW(pFile, FontResource.FR_PRIVATE, null);

            var font = Gdi32.CreateFontW(24, 00, 0, 0,
                FontWeight.FW_NORMAL,
                0, 0, 0,
                Charset.DEFAULT_CHARSET,
                InOutPrecision.OUT_DEFAULT_PRECIS,
                ClipPrecision.CLIP_DEFAULT_PRECIS,
                Quality.ANTIALIASED_QUALITY,
                PitchAndFamily.MONO_FONT | PitchAndFamily.FF_DONTCARE,
                pTypeFace
            );
            Gdi32.SelectObject(deviceContext, font);
            //Gdi32.EnumFontFamiliesW(deviceContext, null, &FJupp, null);
        }


        Gdi32.SetBkMode(deviceContext, BkMode.TRANSPARENT);
        var fromRgb = COLORREF.FromRGB(255, 255, 255);
        //Gdi32.SetBkColor(deviceContext, fromRgb);
        Gdi32.SetTextColor(deviceContext, fromRgb);

        var width = 1024;
        var height = 1024;
        BITMAPINFO info = default;
        info.bmiHeader = new()
        {
            biBitCount = 32,
            biClrImportant = 0,
            biClrUsed = 0,
            biCompression = BICompression.BI_RGB,
            biHeight = height,
            biWidth = width,
            biPlanes = 1,
            biSize = (uint)sizeof(BITMAPINFOHEADER),
            biSizeImage = 0,
            biXPelsPerMeter = 0,
            biYPelsPerMeter = 0
        };

        void* bits;
        var bitmap = Gdi32.CreateDIBSection(deviceContext, &info, DibColorIdentifiers.DIB_RGB_COLORS, &bits, default, 0);
        Gdi32.SelectObject(deviceContext, bitmap);

        foreach (var character in metadata.Characters)
        {
            SIZE size;
            //if (!Gdi32.GetTextExtentPoint32W(deviceContext, &character, 1, &size))
            //{
            //    Logger.Error<FontProcessor>($"Failed to get ExtentPoint. Character = {character}");
            //}
            //else
            //{
            //    Logger.Info<FontProcessor>($"Character = {character}. Size.X = {size.X} Size.Y = {size.Y}");
            //}
        }

        fixed (char* pText = metadata.Characters)
        {
            Gdi32.TextOutW(deviceContext, 0, 0, pText, metadata.Characters.Length);

        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var woop = Image.FromHbitmap(bitmap);
            woop.Save(@$"c:\tmp\bitmap{metadata.Id.ToString()[..6]}.bmp");
        }

        return Task.CompletedTask;
    }

    [UnmanagedCallersOnly]
    private static unsafe int EnumerateFonts(LOGFONTW* font, TEXTMETRICW* metric, uint dword, void* param)
    {
        var str = new string(font->lfFaceName, 0, LOGFONTW.LF_FACESIZE);
        Console.WriteLine($"Font: {str}");
        return 1;
    }
}

