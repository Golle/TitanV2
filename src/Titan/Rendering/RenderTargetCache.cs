using System.Diagnostics;
using Titan.Core;
using Titan.Core.Logging;
using Titan.Core.Strings;
using Titan.Graphics.D3D12;
using Titan.Platform.Win32.DXGI;
using Titan.Resources;
using Titan.Systems;
using Titan.Windows;

namespace Titan.Rendering;

[UnmanagedResource]
internal unsafe partial struct RenderTargetCache
{
    private SpinLock _lock;
    private D3D12ResourceManager* _resourceManager;
    private DXGISwapchain* _swapchain;
    private Window* _window;

    private Inline16<CachedResource> _resources;
    private uint _count;


    [System(SystemStage.PreInit)]
    public static void PreInit(ref RenderTargetCache tracker, UnmanagedResourceRegistry registry)
    {
        tracker._resourceManager = registry.GetResourcePointer<D3D12ResourceManager>();
        tracker._swapchain = registry.GetResourcePointer<DXGISwapchain>();
        tracker._window = registry.GetResourcePointer<Window>();
    }

    public Handle<Texture> GetOrCreateRenderTarget(in RenderTargetConfig targetConfig)
    {
        if (targetConfig.Format == RenderTargetFormat.BackBuffer)
        {
            Logger.Trace<RenderTargetCache>($"Using backbuffer. Name = {targetConfig.Name} Handle = {_swapchain->CurrentBackbuffer.Value}");
            return _swapchain->CurrentBackbuffer;
        }

        var gotLock = false;
        _lock.Enter(ref gotLock);
        Debug.Assert(gotLock);

        try
        {
            var format = targetConfig.Format.AsDXGIFormat();

            // check all cached resources
            for (var i = 0; i < _count; ++i)
            {
                if (_resources[i].Identifier != targetConfig.Name)
                {
                    continue;
                }
                Debug.Assert(_resources[i].Format == format, $"Mismatch in formats for render target with ID = {targetConfig.Name.GetString()}");
                Logger.Trace<RenderTargetCache>($"Found cached render target. Name = {targetConfig.Name} Handle = {_resources[i].Resource.Value}  Format = {format}");
                return _resources[i].Resource;
            }

            // if there are no cached resource, create a new one
            var handle = _resourceManager->CreateTexture(new CreateTextureArgs
            {
                Format = format,
                Height = (uint)_window->Height,
                Width = (uint)_window->Width,
                RenderTargetView = true,
                ShaderVisible = true
            });

            // well, we're in no luck here. This will crash the engine.
            if (handle.IsInvalid)
            {
                Logger.Error<RenderTargetCache>("Failed to create the Render target resource.");
                return Handle<Texture>.Invalid;
            }

            Logger.Trace<RenderTargetCache>($"Created new render target. Name = {targetConfig.Name} Handle = {handle} Format = {format}");
            // Store the resource with its format and identifier
            _resources[_count++] = new()
            {
                Resource = handle,
                Format = format,
                Identifier = targetConfig.Name,
            };

            return handle;
        }
        finally
        {
            _lock.Exit();
        }
    }

    private struct CachedResource
    {
        public StringRef Identifier;
        public DXGI_FORMAT Format;
        public Handle<Texture> Resource;
    }
}
