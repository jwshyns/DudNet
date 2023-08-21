using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DudNet.Generation;

public class Parser
{
    public static bool IsValidTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    public static INamedTypeSymbol? GetSemanticTargetForGeneration(IGeneratorSyntaxContextWrapper context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
    }
}