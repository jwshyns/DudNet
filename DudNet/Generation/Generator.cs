using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DudNet.Generation;

public static class Generator
{
    public static void GenerateProxyService(
        SourceProductionContext context,
        string usings,
        string className,
        string interfaceName,
        string assemblyName,
        List<IMethodSymbol> methods
    )
    {
        var newClassName = $"{className}Proxy";
        var methodStrings = methods.Select(GetMethodString).Where(x => x is not null);
        var interceptorStrings = methods.Select(GetInterceptorMethodString);

        var stringBuilder = new StringBuilder()
            .AppendLine("using System.Runtime.CompilerServices;")
            .AppendLine(usings)
            .AppendLine()
            .AppendLine($"namespace {assemblyName};")
            .AppendLine()
            .AppendLine($"public partial class {newClassName} : {interfaceName} {{")
            .AppendLine()
            .AppendLine($"\tprivate readonly {interfaceName} _service;")
            .AppendLine()
            .AppendLine(string.Join("\t", methodStrings))
            .AppendLine("\tpartial void Interceptor([CallerMemberName]string callerName = null);\n")
            .AppendLine($"\t{string.Join("\n\t", interceptorStrings)}")
            .Append('}');

        context.AddSource($"{newClassName}.g.cs", stringBuilder.ToString());
    }

    public static List<string> GetUsingDirectivesForTypeFile(INamedTypeSymbol namedTypeSymbol)
    {
        var usingDirectivesList = new List<string>();

        // Get a syntax reference (location of the type in source)
        var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();

        if (syntaxReference is null)
        {
            return usingDirectivesList;
        }

        // Get the CompilationUnitSyntax (root of the file)
        var root = syntaxReference.SyntaxTree.GetCompilationUnitRoot();

        // Retrieve all using directives in the file
        usingDirectivesList.AddRange(root.Usings.Select(usingDirective =>
            usingDirective.NormalizeWhitespace().ToFullString()));

        return usingDirectivesList;
    }

    private static string? GetMethodString(IMethodSymbol methodSymbol)
    {
        var methodString = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToFullString();
        return methodString?.Replace(";", GetMethodStringBody(methodSymbol));
    }

    private static string GetMethodStringBody(IMethodSymbol methodSymbol)
    {
        var methodName = methodSymbol.Name;
        var methodArgumentString = string.Join(", ", methodSymbol.Parameters.Select(x => x.Name));
        var stringBuilder = new StringBuilder()
            .AppendLine("{")
            .AppendLine("\t\tInterceptor();")
            .AppendLine($"\t\t{methodName}Interceptor({methodArgumentString});")
            .AppendLine($"\t\t_service.{methodName}({methodArgumentString});")
            .Append("\t}");

        return stringBuilder.ToString();
    }

    private static string GetInterceptorMethodString(IMethodSymbol methodSymbol)
    {
        var name = methodSymbol.Name;
        var parameters = GetParameterListAsString(methodSymbol.Parameters);
        var returnType = methodSymbol.ReturnType;

        return $"partial {returnType} {name}Interceptor({parameters});\n";
    }

    private static string GetParameterListAsString(ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        return string.Join(", ", parameterSymbols.Select(x => $"{x.Type} {x.Name}"));
    }

    public static void GenerateDudService(
        SourceProductionContext context,
        string usings,
        string className,
        string interfaceName,
        string assemblyName,
        List<IMethodSymbol> methods
    )
    {
        var newClassName = $"{className}Dud";
        var methodStrings = methods
            .Select(x => x.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToFullString().Replace(";", "{}"));

        var stringBuilder = new StringBuilder()
            .AppendLine(usings)
            .AppendLine()
            .AppendLine($"namespace {assemblyName};")
            .AppendLine()
            .AppendLine($"public class {newClassName} : {interfaceName} {{")
            .AppendLine()
            .AppendLine(string.Join("\t", methodStrings))
            .AppendLine("}");

        context.AddSource($"{newClassName}.g.cs", stringBuilder.ToString());
    }
}