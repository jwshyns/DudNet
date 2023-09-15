using Microsoft.CodeAnalysis;

namespace DudNet.Tests;

// Courtesy of https://github.com/jmarolf/generator-start/blob/main/tests/Adapter.cs

public class SourceGeneratorAdapter<TIncrementalGenerator> : ISourceGenerator, IIncrementalGenerator
    where TIncrementalGenerator : IIncrementalGenerator, new()
{
    private readonly TIncrementalGenerator _internalGenerator;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public SourceGeneratorAdapter()
    {
        _internalGenerator = new TIncrementalGenerator();
    }

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