using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

public interface IGeneratorSyntaxContextWrapper
{
    public SyntaxNode Node { get; }
    public SemanticModel SemanticModel { get; }
}