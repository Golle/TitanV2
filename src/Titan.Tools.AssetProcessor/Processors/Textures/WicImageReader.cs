using System.Runtime.InteropServices;
using Titan.Core.Logging;
using Titan.Platform.Win32;
using Titan.Platform.Win32.DXGI;
using Titan.Platform.Win32.WIC;

namespace Titan.Tools.AssetProcessor.Processors.Textures;

internal unsafe class WicImageReader : IDisposable
{
    private ComPtr<IWICImagingFactory> _factory;
    public WicImageReader()
    {
        ComPtr<IWICImagingFactory> factory = default;
        var hr = Ole32.CoCreateInstance(CLSID.CLSID_WICImagingFactory2, null, CLSCTX.CLSCTX_INPROC_SERVER, _factory.UUID, (void**)factory.GetAddressOf());
        if (Win32Common.FAILED(hr) || !factory.IsValid)
        {
            throw new Exception($"Failed to create the {nameof(IWICImagingFactory)} instance with HRESULT {hr}");
        }

        //NOTE(Jens): Must use a temporary variable since this is a class. it mooooves because GC..
        _factory = factory;
    }

    public Image? LoadImage(string path)
    {
        using ComPtr<IWICBitmapDecoder> decoder = default;
        HRESULT hr;
        fixed (char* pPath = path)
        {
            hr = _factory.Get()->CreateDecoderFromFilename(pPath, null, (uint)GenericRights.GENERIC_READ, WICDecodeOptions.WICDecodeMetadataCacheOnDemand, decoder.GetAddressOf());
            if (Win32Common.FAILED(hr))
            {
                Logger.Error<WicImageReader>($"Failed to create the {nameof(IWICBitmapDecoder)} with HRESULT {hr}");
                return null;
            }
        }

        using ComPtr<IWICBitmapFrameDecode> frameDecode = default;
        hr = decoder.Get()->GetFrame(0, frameDecode.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to get the frame with HRESULT {hr}");
            return null;
        }

        Guid pixelFormat;
        hr = frameDecode.Get()->GetPixelFormat(&pixelFormat);
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to get the PixelFormat with HRESULT {hr}");
            return null;
        }

        uint height, width;
        hr = frameDecode.Get()->GetSize(&width, &height);
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to get the image size with HRESULT {hr}");
            return null;
        }

        var dxgiFormat = WICToDXGITranslationTable.Translate(pixelFormat);
        if (dxgiFormat == DXGI_FORMAT.DXGI_FORMAT_UNKNOWN)
        {
            Logger.Error<WicImageReader>($"Can't load the pixel, a conversion is required. This is not implemented yet.");
            return null;
        }
        Logger.Trace<WicImageReader>($"Height: {height} Width: {width} DXGI_FORMAT: {dxgiFormat}");


        if (!GetBitsPerPixel(pixelFormat, out var bitsPerPixel))
        {
            Logger.Error<WicImageReader>("Failed to get the BitsPerPixel.");
            return null;
        }

        var stride = (width * bitsPerPixel + 7) / 8;
        // Align the RowPitch/stride to 256 bits.
        var alignedStride = (uint)((stride + 255) & ~255);
        var imageSize = alignedStride * height;

        Logger.Trace<WicImageReader>($"Size: {imageSize} Stride: {stride} Aligned Stride: {alignedStride} BitsPerPixel: {bitsPerPixel}");
        var buffer = new byte[imageSize];
        fixed (byte* pBuffer = buffer)
        {
            hr = frameDecode.Get()->CopyPixels(null, alignedStride, imageSize, pBuffer);
            if (Win32Common.FAILED(hr))
            {
                Logger.Error<WicImageReader>($"Failed to CopyPixels with HRESULT {hr}");
                return null;
            }
        }
        return new Image
        {
            Height = height,
            Width = width,
            BitsPerPixel = bitsPerPixel,
            Data = buffer,
            Format = dxgiFormat,
            Stride = alignedStride
        };
    }
    private bool GetBitsPerPixel(Guid guid, out uint bitsPerPixel)
    {
        bitsPerPixel = default;

        using ComPtr<IWICComponentInfo> componentInfo = default;
        var hr = _factory.Get()->CreateComponentInfo(&guid, componentInfo.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to CreateComponentInfo with HRESULT {hr}");
            return false;
        }
        WICComponentType type;
        hr = componentInfo.Get()->GetComponentType(&type);
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to GetComponentType with HRESULT {hr}");
            return false;
        }
        if (type != WICComponentType.WICPixelFormat)
        {
            Logger.Error<WicImageReader>($"Only {nameof(WICComponentType.WICPixelFormat)} is supported. Got {type}.");
            return false;
        }

        var pixelFormatInfoGuid = typeof(IWICPixelFormatInfo).GUID;
        using ComPtr<IWICPixelFormatInfo> info = default;
        hr = componentInfo.Get()->QueryInterface(&pixelFormatInfoGuid, (void**)info.GetAddressOf());
        if (Win32Common.FAILED(hr))
        {
            Logger.Error<WicImageReader>($"Failed to QueryInterface for {nameof(IWICPixelFormatInfo)} with HRESULT {hr}");
            return false;
        }

        fixed (uint* pBitsPerPixel = &bitsPerPixel)
        {
            hr = info.Get()->GetBitsPerPixel(pBitsPerPixel);
            if (Win32Common.FAILED(hr))
            {
                Logger.Error<WicImageReader>($"Failed to GetBitsPerPixel with HRESULT {hr}");
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
