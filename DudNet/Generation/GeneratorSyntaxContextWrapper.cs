using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

public class GeneratorSyntaxContextWrapper : IGeneratorSyntaxContextWrapper
{
    private readonly GeneratorSyntaxContext _context;

    public GeneratorSyntaxContextWrapper(GeneratorSyntaxContext context)
    {
        _context = context;
    }

    public SyntaxNode Node => _context.Node;
    public SemanticModel SemanticModel => _context.SemanticModel;
}