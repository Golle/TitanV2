using System.Diagnostics;
using System.Runtime.CompilerServices;
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
        tracker._resources = default;
    }

    public Handle<Texture> GetOrCreateDepthBuffer(in DepthBufferConfig depthConfig)
    {
        var gotLock = false;
        _lock.Enter(ref gotLock);
        Debug.Assert(gotLock);

        try
        {
            var format = depthConfig.Format.AsDXGIFormat();

            if (TryGetCachedResource(depthConfig.Name, format, out var existingHandle))
            {
                return existingHandle;
            }

            var handle = _resourceManager->CreateDepthBuffer(new CreateDepthBufferArgs
            {
                Format = format,
                Height = (uint)_window->Height,
                Width = (uint)_window->Width,
                ClearValue = depthConfig.ClearValue
            });
            if (handle.IsInvalid)
            {
                Logger.Error<RenderTargetCache>("Failed to create the DeptBuffer resource.");
                return Handle<Texture>.Invalid;
            }
            Logger.Trace<RenderTargetCache>($"Created new DepthBuffer. Name = {depthConfig.Name} Handle = {handle} Format = {format}");
            // Store the resource with its format and identifier
            _resources[_count++] = new()
            {
                Resource = handle,
                Format = format,
                Identifier = depthConfig.Name,
            };

            return handle;
        }
        finally
        {
            _lock.Exit();
        }
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

            if (TryGetCachedResource(targetConfig.Name, format, out var existingHandle))
            {
                return existingHandle;
            }

            // if there are no cached resource, create a new one
            var handle = _resourceManager->CreateTexture(new CreateTextureArgs
            {
                Format = format,
                Height = (uint)_window->Height,
                Width = (uint)_window->Width,
                RenderTargetView = true,
                ShaderVisible = true,
                OptimizedClearColor = targetConfig.OptimizedClearColor
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

    public bool TryGetCachedResource(StringRef name, DXGI_FORMAT format, out Handle<Texture> handle)
    {
        Unsafe.SkipInit(out handle);
        for (var i = 0; i < _count; ++i)
        {
            if (_resources[i].Identifier != name)
            {
                continue;
            }
            Debug.Assert(_resources[i].Format == format, $"Mismatch in formats for resource with ID = {name.GetString()}");
            Logger.Trace<RenderTargetCache>($"Found cached resource. Name = {name.GetString()} Handle = {_resources[i].Resource.Value} Format = {format}");
            handle = _resources[i].Resource;
            return true;
        }

        return false;
    }

    [System(SystemStage.Shutdown)]
    public static void Shutdown(RenderTargetCache* cache, in D3D12ResourceManager resourceManager, in DXGISwapchain _)
    {
        //NOTE(Jens): adding a dependency to Swapchain will prevent a crash from happening. I think we need to do this in some other way if we want to support cleaning up.
        for (var i = 0; i < cache->_count; ++i)
        {
            resourceManager.DestroyTexture(cache->_resources[i].Resource);
        }

        *cache = default;
    }
    private struct CachedResource
    {
        public StringRef Identifier;
        public DXGI_FORMAT Format;
        public Handle<Texture> Resource;
    }
}
