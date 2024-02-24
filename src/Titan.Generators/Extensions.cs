using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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


    public static bool IsStruct(this SyntaxNode node)
        => node is StructDeclarationSyntax;

    public static bool IsRecordStruct(this SyntaxNode node)
        => node is RecordDeclarationSyntax recordDecl && recordDecl.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);

    public static bool IsPartial(this SyntaxNode node)
        => node is TypeDeclarationSyntax type && type.Modifiers.Any(static m => m.IsKind(SyntaxKind.PartialKeyword));
}
