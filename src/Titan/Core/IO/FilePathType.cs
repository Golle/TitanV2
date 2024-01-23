namespace Titan.Core.IO;

public enum FilePathType
{
    /// <summary>
    /// The Path to the AppData that configs etc can be stored in
    /// </summary>
    AppData,
    /// <summary>
    /// The Content path for the game
    /// </summary>
    Content,
    /// <summary>
    /// The Path to the engine Content, this will be the same as Content on release builds. 
    /// </summary>
    Engine,
    /// <summary>
    /// A Temp folder that lives inside the AppData
    /// </summary>
    Temp,
    /// <summary>
    /// Logs folder that lives inside the AppData
    /// </summary>
    Logs,

    /// <summary>
    /// The number of types
    /// </summary>
    Count
}
