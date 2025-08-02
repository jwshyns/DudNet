using Microsoft.CodeAnalysis;

namespace DudNet.Tests.Generation;

// Courtesy of https://github.com/jmarolf/generator-start/blob/main/tests/Adapter.cs
#pragma warning disable RS1042
internal sealed class SourceGeneratorAdapter<TIncrementalGenerator> : ISourceGenerator, IIncrementalGenerator
#pragma warning restore RS1042
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    private readonly TIncrementalGenerator _internalGenerator = new();

    public void Execute(GeneratorExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        throw new NotImplementedException();
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        _internalGenerator.Initialize(context);
    }
}