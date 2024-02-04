using Microsoft.CodeAnalysis;

namespace Titan.Generators;
internal static class Extensions
{

    public static string AsString(this Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            _ => "public"
        };
}
