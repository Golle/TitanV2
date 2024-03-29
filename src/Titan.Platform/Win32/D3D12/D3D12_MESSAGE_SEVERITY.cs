namespace Titan.Platform.Win32.D3D12;

public enum D3D12_MESSAGE_SEVERITY
{
    D3D12_MESSAGE_SEVERITY_CORRUPTION = 0,
    D3D12_MESSAGE_SEVERITY_ERROR = (D3D12_MESSAGE_SEVERITY_CORRUPTION + 1),
    D3D12_MESSAGE_SEVERITY_WARNING = (D3D12_MESSAGE_SEVERITY_ERROR + 1),
    D3D12_MESSAGE_SEVERITY_INFO = (D3D12_MESSAGE_SEVERITY_WARNING + 1),
    D3D12_MESSAGE_SEVERITY_MESSAGE = (D3D12_MESSAGE_SEVERITY_INFO + 1)
}
