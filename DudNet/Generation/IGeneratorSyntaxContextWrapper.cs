using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

internal interface IGeneratorSyntaxContextWrapper
{
    /// <summary>
    ///     The node being evaluated.
    /// </summary>
    public SyntaxNode Node { get; }

    /// <summary>
    ///     The semantic model for context.
    /// </summary>
    public SemanticModel SemanticModel { get; }
}