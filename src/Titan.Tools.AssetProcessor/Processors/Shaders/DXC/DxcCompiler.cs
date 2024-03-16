using System.Runtime.CompilerServices;
using System.Text;
using Titan.Core.Logging;
using Titan.Platform.DXC;
using Titan.Platform.Win32;
using static Titan.Platform.Win32.Win32Common;


namespace Titan.Tools.AssetProcessor.Processors.Shaders.DXC;
file unsafe class DxcCompilerResult : ShaderCompilationResult
{
    private ComPtr<IDXCBlob> _byteCode;
    public DxcCompilerResult(string error)
        : base(error)
    {
    }

    public DxcCompilerResult(ComPtr<IDXCBlob> byteCode)
    {
        // create a new instance to call AddRef on the ComObject
        _byteCode = new ComPtr<IDXCBlob>(byteCode);
    }

    public override ReadOnlySpan<byte> GetByteCode()
    {
        var ptr = _byteCode.Get();
        if (ptr == null || ptr->GetBufferPointer() == null || ptr->GetBufferSize() == 0)
        {
            return ReadOnlySpan<byte>.Empty;
        }
        return new ReadOnlySpan<byte>(ptr->GetBufferPointer(), (int)ptr->GetBufferSize());
    }
    public override void Dispose() => _byteCode.Dispose();
}


internal unsafe class DxcCompiler : IShaderCompiler
{
    public bool IsSupported(ShaderVersion version)
        => version <= ShaderVersion.Version_6_0;
    public ShaderCompilationResult CompileShader(string filePath, string entryPoint, ShaderType type, ShaderVersion version)
    {
        using ComPtr<IDXCUtils> utils = default;
        using ComPtr<IDXCCompiler3> compiler = default;
        using ComPtr<IDXCIncludeHandler> includeHandler = default;

        HRESULT hr;

        hr = DXCCompilerCommon.DxcCreateInstance(DXCCompilerCommon.CLSID_DxcUtils, IDXCUtils.Guid, (void**)utils.GetAddressOf());
        if (FAILED(hr))
        {
            var errorMessage = $"Failed to create and instance of {nameof(IDXCUtils)} with HRESULT {hr}";
            Logger.Error<DxcCompiler>(errorMessage);
            return new DxcCompilerResult(errorMessage);
        }

        hr = DXCCompilerCommon.DxcCreateInstance(DXCCompilerCommon.CLSID_Compiler, IDXCCompiler3.Guid, (void**)compiler.GetAddressOf());
        if (FAILED(hr))
        {
            var errorMessage = $"Failed to create and instance of {nameof(IDXCCompiler3)} with HRESULT {hr}";
            Logger.Error<DxcCompiler>(errorMessage);
            return new DxcCompilerResult(errorMessage);
        }

        hr = utils.Get()->CreateDefaultIncludeHandler(includeHandler.GetAddressOf());
        if (FAILED(hr))
        {
            var errrMessage = $"Failed to create the default include handler with HRESULT {hr}";
            Logger.Error<DxcCompiler>(errrMessage);
            return new DxcCompilerResult(errrMessage);
        }


        using ComPtr<IDXCBlobEncoding> source = default;
        fixed (char* pFilePath = filePath)
        {
            hr = utils.Get()->LoadFile(pFilePath, null, source.GetAddressOf());
            if (FAILED(hr))
            {
                var errorMessage = $"Failed to read the file contents with HRESULT {hr}";
                Logger.Error<DxcCompiler>(errorMessage);
                return new DxcCompilerResult(errorMessage);
            }
        }

        var includePath = Path.GetDirectoryName(filePath);

        var args = new CompilerArgs(stackalloc char[2048]); // 2048 characters, 4kb stack allocation.
        args.AddArgument(Path.GetFileName(filePath)); // add the file name for debugging
        args.AddArgument("-E");
        args.AddArgument(entryPoint);
        args.AddArgument("-T");
        args.AddArgument(ShaderToString(type, version));
        args.AddArgument("-Qstrip_reflect");
        args.AddArgument("-Qstrip_debug");
        args.AddArgument("-O3");
        args.AddArgument("-all_resources_bound");
        args.AddArgument("-WX");
        args.AddArgument($"-I {includePath}"); // include dir


        //NOTE(Jens): add more arguments when we want to do debug builds, pdbs etc.


        var buffer = new DXCBuffer
        {
            Encoding = 0, // assume default encoding (UTF8 with/wothout BOM)
            Ptr = source.Get()->GetBufferPointer(),
            Size = source.Get()->GetBufferSize()
        };

        using ComPtr<IDXCResult> result = default;
        using ComPtr<IDXCBlobWide> compileErrors = default;
        hr = compiler.Get()->Compile(&buffer, args.GetArguments(), args.GetArgumentsCount(), includeHandler.Get(), IDXCResult.Guid, (void**)result.GetAddressOf());
        if (FAILED(hr))
        {
            Logger.Error<DxcCompiler>($"Compile failed with HRESULT {hr}");
            return new DxcCompilerResult($"Compile failed with HRESULT {hr}");
        }

        result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_ERRORS, IDXCBlobWide.Guid, (void**)compileErrors.GetAddressOf(), null);
        var hasErrorsOrWarnings = compileErrors.Get() != null && compileErrors.Get()->GetStringLength() != 0 && compileErrors.Get()->GetBufferPointer() != null;
        if (hasErrorsOrWarnings)
        {
            Logger.Warning<DxcCompiler>(BlobToString(compileErrors));
        }


        using ComPtr<IDXCBlobWide> remarks = default;
        result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_REMARKS, IDXCBlobWide.Guid, (void**)remarks.GetAddressOf(), null);
        if (remarks.Get() != null)
        {
            Logger.Warning<DxcCompiler>(BlobToString(remarks));
        }

        result.Get()->GetStatus(&hr);
        if (FAILED(hr))
        {
            var errorMessage = $"Compile returned S_OK but the compilation failed with HRESULT {hr}";
            Logger.Error<DxcCompiler>(errorMessage);
            using ComPtr<IDXCBlobEncoding> errors = default;
            result.Get()->GetErrorBuffer(errors.GetAddressOf());
            if (errors.Get() != null && errors.Get()->GetBufferPointer() != null && errors.Get()->GetBufferSize() > 0)
            {
                var error = Encoding.UTF8.GetString((byte*)errors.Get()->GetBufferPointer(), (int)errors.Get()->GetBufferSize());
                return new DxcCompilerResult(error);
            }

            return hasErrorsOrWarnings ? new DxcCompilerResult(BlobToString(compileErrors)) : new DxcCompilerResult(errorMessage);
        }

        using ComPtr<IDXCBlob> byteCode = default;
        result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_OBJECT, byteCode.UUID, (void**)byteCode.GetAddressOf(), null/* shader name if we want it*/);
        if (byteCode.Get() != null && byteCode.Get()->GetBufferSize() > 0)
        {
            Logger.Info<DxcCompiler>("Compilation successful!");
            return new DxcCompilerResult(byteCode);
        }

        return new DxcCompilerResult("The compiled shader has the size 0.");

        static string BlobToString(in ComPtr<IDXCBlobWide> blob) =>
            new((char*)blob.Get()->GetBufferPointer(), 0, (int)blob.Get()->GetBufferSize());


        static string ShaderToString(ShaderType type, ShaderVersion version)
        {
            var shader = type switch
            {
                ShaderType.Compute => "cs",
                ShaderType.Vertex => "vs",
                ShaderType.Pixel => "ps",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            var versionStr = version
                .ToString()
                .Split('_', 2)[1];

            return $"{shader}_{versionStr}";
        }
    }

    /// <summary>
    /// struct used to simply the WCHAR** data type used for compiler arguments.
    /// </summary>
    private ref struct CompilerArgs
    {
        private const int MaxArguments = 100;
        private fixed ulong _arguments[MaxArguments];
        private Span<char> _buffer;
        private int _index;
        private uint _count;
        public CompilerArgs(Span<char> buffer)
        {
            _buffer = buffer;
        }

        public void AddArgument(string arg)
        {
            var nextIndex = _index + arg.Length + 1;
            arg.CopyTo(_buffer[_index..]);
            _arguments[_count++] = (ulong)Unsafe.AsPointer(ref _buffer[_index]);
            _index = nextIndex;
        }

        public char** GetArguments() => (char**)Unsafe.AsPointer(ref _arguments[0]);
        public uint GetArgumentsCount() => _count;
    }
}
