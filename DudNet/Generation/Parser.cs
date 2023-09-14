using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DudNet.Generation;

/// <summary>
/// Functionality for parsing for appropriate source generation targets.
/// </summary>
internal class Parser
{
    /// <summary>
    /// Determines whether a particular <see cref="SyntaxNode"/> is potential target for proxy service generation.
    /// </summary>
    /// <param name="node">The node to evaluate.</param>
    /// <returns><c>True</c> if the node is a potential target, <c>False</c> if not.</returns>
    public static bool IsPotentialTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }
    
    /// <summary>
    /// Gets the <see cref="INamespaceSymbol"/> for a provided <see cref="IGeneratorSyntaxContextWrapper"/>.
    /// </summary>
    /// <param name="context">The provided context wrapper.</param>
    /// <returns>The appropriate <see cref="INamespaceSymbol"/> for the provided context.</returns>
    public static INamedTypeSymbol? GetSemanticTargetForGeneration(IGeneratorSyntaxContextWrapper context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        return context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
    }
}