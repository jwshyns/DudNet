using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

public interface IGeneratorSyntaxContextWrapper
{
    /// <summary>
    /// The node being evaluated.
    /// </summary>
    public SyntaxNode Node { get; }
    
    /// <summary>
    /// The semantic model for context.
    /// </summary>
    public SemanticModel SemanticModel { get; }
}