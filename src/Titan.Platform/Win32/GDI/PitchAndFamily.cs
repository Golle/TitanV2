namespace Titan.Platform.Win32.GDI;

[Flags]
public enum PitchAndFamily : uint
{
    DEFAULT_PITCH = 0,
    FIXED_PITCH = 1,
    VARIABLE_PITCH = 2,
    MONO_FONT = 8,

    FF_DONTCARE = 0, /* Don't care or don't know. */
    FF_ROMAN = (1 << 4), /* Variable stroke width, serifed. */
    FF_SWISS = (2 << 4), /* Variable stroke width, sans-serifed. */
    FF_MODERN = (3 << 4), /* Constant stroke width, serifed or sans-serifed. */
    FF_SCRIPT = (4 << 4), /* Cursive, etc. */
    FF_DECORATIVE = (5 << 4), /* Old English, etc. */
}