using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DudNet.Generation;

[Generator]
public class ProxyServiceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(static (s, _) => Parser.IsValidTarget(s),
                (ctx, _) => Parser.GetSemanticTargetForGeneration(new GeneratorSyntaxContextWrapper(ctx)))
            .Where(static symbol => symbol is not null)!;

        var compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(
        Compilation _,
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
            if (target.Interfaces[0] is not { } interfaceSymbol)
            {
                continue;
            }

            var interfaceName = interfaceSymbol.Name;

            var assemblyName = target.ContainingAssembly.Name;
            var name = target.Name;
            var methods = interfaceSymbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Method)
                .OfType<IMethodSymbol>()
                .ToList();

            var usings = string.Join("", Generator.GetUsingDirectivesForTypeFile(target));

            Generator.GenerateProxyService(context, usings, name, interfaceName, assemblyName, methods);
            Generator.GenerateDudService(context, usings, name, interfaceName, assemblyName, methods);
        }
    }
}