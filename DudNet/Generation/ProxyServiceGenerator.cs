using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

/// <summary>
/// A source generator for generating a proxied service.
/// </summary>
[Generator]
public class ProxyServiceGenerator : IIncrementalGenerator
{

    /// <inheritdoc cref="IIncrementalGenerator.Initialize"/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(static (s, _) => Parser.IsPotentialTarget(s),
                (ctx, _) => Parser.GetSemanticTargetForGeneration(new GeneratorSyntaxContextWrapper(ctx)))
            .Where(static symbol => symbol is not null)!;

        var compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Right, spc));
    }
    
    /// <summary>
    /// Performs further filtering of source generation targets and performs the actual generation.
    /// </summary>
    /// <param name="targets">Potential targets for source generation.</param>
    /// <param name="context">Context for source generation.</param>
    private static void Execute(
        ImmutableArray<INamedTypeSymbol> targets,
        SourceProductionContext context
    )
    {
        if (targets.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var target in targets)
        {
            // targets must implement an interface
            if (target.Interfaces[0] is not { } interfaceSymbol)
            {
                continue;
            }

            var interfaceName = interfaceSymbol.Name;

            var assemblyName = target.ContainingAssembly.Name;
            var name = target.Name;
            
            // collect methods on the interface
            var methods = interfaceSymbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .OfType<IMethodSymbol>()
                .ToList();
            
            // generates a string representation of necessary usings (imports)
            var usings = string.Join("", Generator.GetUsingDirectivesForTypeFile(target));
            
            // generate the source files
            Generator.GenerateProxyService(context, usings, name, interfaceName, assemblyName, methods);
            Generator.GenerateDudService(context, usings, name, interfaceName, assemblyName, methods);
        }
    }
}