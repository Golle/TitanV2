namespace Titan.Rendering;

public enum RenderPassType
{
    /// <summary>
    /// Only a single scene pass can exist
    /// </summary>
    Scene,

    /// <summary>
    /// Only a single DeferredLighting pass can exist
    /// </summary>
    DeferredLighting,

    /// <summary>
    /// Only a single Backbuffer pass can exist
    /// </summary>
    Backbuffer,

    /// <summary>
    /// The custom render pass type can be used when lookups should be based on the name.
    /// </summary>
    Custom
}
