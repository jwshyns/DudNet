using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

/// <summary>
/// A wrapper for making unit testing more convenient.
/// </summary>
public class GeneratorSyntaxContextWrapper : IGeneratorSyntaxContextWrapper
{
    /// <summary>
    /// The generation context.
    /// </summary>
    private readonly GeneratorSyntaxContext _context;

    public GeneratorSyntaxContextWrapper(GeneratorSyntaxContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// The node being evaluated.
    /// </summary>
    public SyntaxNode Node => _context.Node;
    
    /// <summary>
    /// The semantic model for context.
    /// </summary>
    public SemanticModel SemanticModel => _context.SemanticModel;
}