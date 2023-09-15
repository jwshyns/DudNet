using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DudNet.Generation;

/// <summary>
/// Functionality for generating source for a proxied service.
/// </summary>
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
internal static class Generator
{
    /// <summary>
    /// Creates source for a proxy service.
    /// </summary>
    /// <param name="context">Context for source generation.</param>
    /// <param name="usings">Usings (imports) for the source.</param>
    /// <param name="className">The name of the service to be proxied.</param>
    /// <param name="interfaceName">The name of the interface the service implements.</param>
    /// <param name="assemblyName">The name of the service's containing assembly.</param>
    /// <param name="methods">The methods the interface defines.</param>
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
        // get string representing the proxy methods
        var methodStrings = methods.Select(GetProxyMethodString).Where(x => x is not null);
        // get string representing the partial interceptors
        var interceptorStrings = methods.Select(GetInterceptorMethodString);

        // build the source
        var stringBuilder = new StringBuilder()
            .AppendLine("using System.Runtime.CompilerServices;")
            .AppendLine(usings)
            .AppendLine($"namespace {assemblyName};")
            .AppendLine()
            .AppendLine($"public partial class {newClassName} : {interfaceName} {{")
            .AppendLine()
            .AppendLine($"\tprivate readonly {interfaceName} _service;")
            .AppendLine()
            .AppendLine(string.Join($"{Environment.NewLine}{Environment.NewLine}", methodStrings))
            .AppendLine()
            .AppendLine($"\tpartial void Interceptor([CallerMemberName]string callerName = null);{Environment.NewLine}")
            .AppendLine($"\t{string.Join($"{Environment.NewLine}\t", interceptorStrings)}")
            .Append('}');

        // add the source to the compilation output
        context.AddSource($"{newClassName}.g.cs", stringBuilder.ToString());
    }

    /// <summary>
    /// Builds a list of string representing usings for the service being proxied.
    /// </summary>
    /// <param name="namedTypeSymbol">The <see cref="INamedTypeSymbol"/> representing the service.</param>
    /// <returns>A list of string representing usings for the service being proxied</returns>
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
            $"{usingDirective.NormalizeWhitespace().ToFullString()}{Environment.NewLine}"));

        return usingDirectivesList;
    }

    /// <summary>
    /// Builds a string presenting a proxied method.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> being proxied.</param>
    /// <returns>A string representing a proxied method.</returns>
    private static string GetProxyMethodString(IMethodSymbol methodSymbol)
    {
        var methodString = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToFullString().Trim();
        return $"\t{methodString?.Replace(";", GetProxyMethodStringBody(methodSymbol))}";
    }

    /// <summary>
    /// Builds a string representing the body of a proxied method.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> being proxied.</param>
    /// <returns>A string representing body of a proxied method.</returns>
    private static string GetProxyMethodStringBody(IMethodSymbol methodSymbol)
    {
        var methodName = methodSymbol.Name;
        var methodArgumentString = string.Join(", ", methodSymbol.Parameters.Select(x => x.Name));
        var returnString = methodSymbol.ReturnsVoid ? "" : "return ";
        
        var stringBuilder = new StringBuilder()
            .AppendLine(" {")
            .AppendLine("\t\tInterceptor();")
            .AppendLine($"\t\t{methodName}Interceptor({methodArgumentString});")
            .AppendLine($"\t\t{returnString}_service.{methodName}({methodArgumentString});")
            .Append("\t}");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Builds a string presenting a dud method.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> being made dud.</param>
    /// <returns>A string representing a dud method.</returns>
    private static string GetDudMethodString(IMethodSymbol methodSymbol)
    {
        var methodString = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().ToFullString().Trim();
        return $"\t{methodString?.Replace(";", GetDudMethodStringBody(methodSymbol))}";
    }
    
    /// <summary>
    /// Builds a string representing the body of a dud method.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol"/> being made dud.</param>
    /// <returns>A string representing body of a dud method.</returns>
    private static string GetDudMethodStringBody(IMethodSymbol methodSymbol)
    {
        return methodSymbol.ReturnsVoid ? " {}" : $" {{{Environment.NewLine}\t\treturn ({methodSymbol.ReturnType}) default;{Environment.NewLine}\t}}{Environment.NewLine}";
    }

    /// <summary>
    /// Builds a string representation of an interceptor for a particular <see cref="IMethodSymbol"/>.
    /// </summary>
    /// <param name="methodSymbol">The method symbol being converted to an interceptor string.</param>
    /// <returns>An interceptor string representation of the provided method.</returns>
    private static string GetInterceptorMethodString(IMethodSymbol methodSymbol)
    {
        var name = methodSymbol.Name;
        var parameters = GetParameterListAsString(methodSymbol.Parameters);

        return $"partial void {name}Interceptor({parameters});{Environment.NewLine}";
    }

    /// <summary>
    /// Converts a <see cref="ImmutableArray{IParameterSymbol}"/> to a string representation.
    /// </summary>
    /// <param name="parameterSymbols">The parameters being converted to a string.</param>
    /// <returns>A string representation of method parameters.</returns>
    private static string GetParameterListAsString(ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        return string.Join(", ", parameterSymbols.Select(x => $"{x.Type} {x.Name}"));
    }

    /// <summary>
    /// Creates source for a dud service.
    /// </summary>
    /// <param name="context">Context for source generation.</param>
    /// <param name="usings">Usings (imports) for the source.</param>
    /// <param name="className">The name of the service to be proxied.</param>
    /// <param name="interfaceName">The name of the interface the service implements.</param>
    /// <param name="assemblyName">The name of the service's containing assembly.</param>
    /// <param name="methods">The methods the interface defines.</param>
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

        // get
        var methodStrings = methods.Select(GetDudMethodString);

        var stringBuilder = new StringBuilder()
            .AppendLine(usings)
            .AppendLine($"namespace {assemblyName};")
            .AppendLine()
            .AppendLine($"public class {newClassName} : {interfaceName} {{")
            .AppendLine()
            .AppendLine(string.Join($"{Environment.NewLine}{Environment.NewLine}", methodStrings))
            .Append("}");

        context.AddSource($"{newClassName}.g.cs", stringBuilder.ToString());
    }
}