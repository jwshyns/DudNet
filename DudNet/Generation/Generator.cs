using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DudNet.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DudNet.Generation;

/// <summary>
///     Functionality for generating source for a proxied service.
/// </summary>
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
internal static class Generator
{
    #region Dud

    /// <summary>
    ///     Creates source for a dud service.
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

        GenerateService(context, usings, newClassName, interfaceName, assemblyName, methods, GenerateDudClassBody);
    }

    /// <summary>
    ///     Generates the body of a dud class.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the class body to.</param>
    /// <param name="methods">Methods that need to be proxied.</param>
    /// <param name="propertyMethodLookup">A look up mapping auto-properties to their getters and setters.</param>
    /// <param name="interfaceName">The name of the interface being made dud.</param>
    private static void GenerateDudClassBody(
        IIndentedStringBuilder stringBuilder,
        List<IMethodSymbol> methods,
        ILookup<IPropertySymbol, IMethodSymbol> propertyMethodLookup,
        string interfaceName
    )
    {
        GenerateProperties(stringBuilder, propertyMethodLookup, GenerateDudMethod);
        GenerateMethods(stringBuilder, methods, GenerateDudMethod, true);
    }

    /// <summary>
    ///     Generates a dud method.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the method to.</param>
    /// <param name="methodSymbol">The method being made dud.</param>
    /// <param name="isPropertyMethod">Whether the method is an auto-property getter or setter.</param>
    private static void GenerateDudMethod(
        IIndentedStringBuilder stringBuilder,
        IMethodSymbol methodSymbol,
        bool isPropertyMethod
    )
    {
        var methodString = GenerateMethodDeclaration(methodSymbol);

        stringBuilder
            .AppendLine($"{methodString} {{")
            .IndentedBlockWrite(sb =>
            {
                if (!methodSymbol.ReturnsVoid) sb.AppendLine($"return ({methodSymbol.ReturnType}) default;");
            })
            .AppendLine('}');
    }

    #endregion

    #region Proxy

    /// <summary>
    ///     Creates source for a proxy service.
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

        GenerateService(context, usings, newClassName, interfaceName, assemblyName, methods, GenerateProxyClassBody);
    }

    /// <summary>
    ///     Generates the body of a proxy class.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the class body to.</param>
    /// <param name="methods">Methods that need to be proxied.</param>
    /// <param name="propertyMethodLookup">A look up mapping auto-properties to their getters and setters.</param>
    /// <param name="interfaceName">The name of the interface being proxied.</param>
    private static void GenerateProxyClassBody(
        IIndentedStringBuilder stringBuilder,
        List<IMethodSymbol> methods,
        ILookup<IPropertySymbol, IMethodSymbol> propertyMethodLookup,
        string interfaceName
    )
    {
        stringBuilder
            .AppendLine($"private readonly {interfaceName} _service;")
            .AppendLine();

        GenerateProperties(stringBuilder, propertyMethodLookup, GenerateProxyMethod);
        GenerateMethods(stringBuilder, methods, GenerateProxyMethod, true);

        stringBuilder
            .AppendLine("partial void Interceptor([CallerMemberName]string callerName = null);")
            .AppendLine()
            .BlockWrite(sb => GenerateInterceptorMethodStrings(sb, methods));
    }

    /// <summary>
    ///     Generates a proxy method.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the method to.</param>
    /// <param name="methodSymbol">The method being proxied.</param>
    /// <param name="isPropertyMethod">Whether the method is an auto-property getter or setter.</param>
    private static void GenerateProxyMethod(
        IIndentedStringBuilder stringBuilder,
        IMethodSymbol methodSymbol,
        bool isPropertyMethod
    )
    {
        var methodString = GenerateMethodDeclaration(methodSymbol);

        var interceptorName = methodSymbol.Name;
        var methodName = interceptorName;
        string interceptorArgumentString, methodArgumentString;
        var returnString = methodSymbol.ReturnsVoid ? string.Empty : "return ";

        if (isPropertyMethod)
        {
            methodName = methodName.Substring(4);
            if (methodSymbol.MethodKind is MethodKind.PropertyGet)
            {
                interceptorArgumentString = "()";
                methodArgumentString = "";
            }
            else
            {
                interceptorArgumentString = "(value)";
                methodArgumentString = " = value";
            }
        }
        else
        {
            interceptorArgumentString = $"({string.Join(", ", methodSymbol.Parameters.Select(x => x.Name))})";
            methodArgumentString = interceptorArgumentString;
        }

        stringBuilder
            .AppendLine($"{methodString} {{")
            .IndentedBlockWrite(sb =>
            {
                sb
                    .AppendLine("Interceptor();")
                    .AppendLine($"{interceptorName}Interceptor{interceptorArgumentString};")
                    .AppendLine($"{returnString}_service.{methodName}{methodArgumentString};");
            })
            .AppendLine('}');
    }

    /// <summary>
    ///     Generates an interceptor for a method being proxied.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the interceptor to.</param>
    /// <param name="methodSymbols">The methods to be intercepted.</param>
    private static void GenerateInterceptorMethodStrings(
        IIndentedStringBuilder stringBuilder,
        IEnumerable<IMethodSymbol> methodSymbols
    )
    {
        foreach (var method in methodSymbols)
        {
            var name = method.Name;
            var parameters = GetParametersAsString(method.Parameters);

            stringBuilder.AppendLine($"partial void {name}Interceptor({parameters});").AppendLine();
        }
    }

    #endregion

    #region Shared

    /// <summary>
    ///     Generates auto-properties.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the properties to.</param>
    /// <param name="propertyMethodLookup">
    ///     A <see cref="ILookup{TKey,TElement}" /> mapping auto-properties to their
    ///     getters and setters
    /// </param>
    /// <param name="methodGenerator">An action used for generating the method.</param>
    private static void GenerateProperties(
        IIndentedStringBuilder stringBuilder,
        ILookup<IPropertySymbol, IMethodSymbol> propertyMethodLookup,
        Action<IIndentedStringBuilder, IMethodSymbol, bool> methodGenerator
    )
    {
        foreach (var grouping in propertyMethodLookup)
        {
            var property = grouping.Key;
            var propertyName = property.Name;
            var propertyReturnType = property.Type;

            stringBuilder
                .AppendLine($"public {propertyReturnType} {propertyName} {{")
                .IndentedBlockWrite(sb => GenerateMethods(sb, grouping.ToList(), methodGenerator))
                .AppendLine('}')
                .AppendLine();
        }
    }

    /// <summary>
    ///     Generates multiple methods source.
    /// </summary>
    /// <param name="stringBuilder">String builder for appending the methods to.</param>
    /// <param name="methodSymbols"><see cref="IMethodSymbol" />s that need to have source generated</param>
    /// <param name="methodGenerator">An action used for generating the methods.</param>
    /// <param name="skipPropertyMethods">Whether auto-property methods should be skipped.</param>
    private static void GenerateMethods(
        IIndentedStringBuilder stringBuilder,
        List<IMethodSymbol> methodSymbols,
        Action<IIndentedStringBuilder, IMethodSymbol, bool> methodGenerator,
        bool skipPropertyMethods = false
    )
    {
        foreach (var method in methodSymbols)
        {
            var isPropertyMethod = method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;

            if (isPropertyMethod && skipPropertyMethods) continue;

            methodGenerator(stringBuilder, method, isPropertyMethod);
            stringBuilder.AppendLine();
        }
    }

    /// <summary>
    ///     Generates a given <see cref="IMethodSymbol" />'s declaration.
    /// </summary>
    /// <param name="methodSymbol">The <see cref="IMethodSymbol" /> being processed.</param>
    /// <returns>A <see cref="string" /> representation of the provided <see cref="IMethodSymbol" /></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when a <see cref="IMethodSymbol" />'s declared accessibility
    ///     is not supported.
    /// </exception>
    private static string GenerateMethodDeclaration(IMethodSymbol methodSymbol)
    {
        var result = new StringBuilder();

        var isPropertyMethod = methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;

        if (!isPropertyMethod)
        {
            // Access Modifier
            switch (methodSymbol.DeclaredAccessibility)
            {
                case Accessibility.Internal:
                    result.Append("internal ");
                    break;
                case Accessibility.Public:
                    result.Append("public ");
                    break;
                case Accessibility.Protected:
                    result.Append("protected ");
                    break;
                case Accessibility.NotApplicable:
                case Accessibility.Private:
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Modifiers
            if (methodSymbol.IsStatic) result.Append("static ");

            // Return Type
            result.Append(methodSymbol.ReturnType + " ");
        }

        // Method Name
        result.Append(isPropertyMethod ? methodSymbol.Name.Split('_')[0] : methodSymbol.Name);

        // Generic Type Parameters
        if (methodSymbol.IsGenericMethod) result.Append($"<{GetTypeParameterAsString(methodSymbol.TypeParameters)}>");

        if (!isPropertyMethod)
            // Parameters
            result.Append($"({GetParametersAsString(methodSymbol.Parameters)})");

        return result.ToString();
    }

    /// <summary>
    ///     Creates a service given the provided parameters, and outputs C# source.
    /// </summary>
    /// <param name="context">Context for source generation.</param>
    /// <param name="usings">Usings (imports) for the source.</param>
    /// <param name="className">The name of the service to be proxied.</param>
    /// <param name="interfaceName">The name of the interface the service implements.</param>
    /// <param name="assemblyName">The name of the service's containing assembly.</param>
    /// <param name="methods">The methods the interface defines.</param>
    /// <param name="classBodyGenerator">An action that is used for generating the service's class body.</param>
    private static void GenerateService(
        SourceProductionContext context,
        string usings,
        string className,
        string interfaceName,
        string assemblyName,
        List<IMethodSymbol> methods,
        Action<IIndentedStringBuilder, List<IMethodSymbol>, ILookup<IPropertySymbol, IMethodSymbol>, string>
            classBodyGenerator
    )
    {
        var newClassName = $"{className}";

        var propertyMethodLookup = GetAutoPropertyMethods(methods);

        // build the source
        var stringBuilder = new IndentedStringBuilder()
            .AppendLine("using System.Runtime.CompilerServices;")
            .AppendLine(usings)
            .AppendLine($"namespace {assemblyName};")
            .AppendLine()
            .AppendLine($"/// <inheritdoc cref=\"{interfaceName}\"/>")
            .AppendLine($"public partial class {newClassName} : {interfaceName} {{")
            .AppendLine()
            .IndentedBlockWrite(sb => classBodyGenerator(sb, methods, propertyMethodLookup, interfaceName))
            .Append('}');

        // add the source to the compilation output
        context.AddSource($"{newClassName}.g.cs", stringBuilder.ToString());
    }

    /// <summary>
    ///     Converts a <see cref="ImmutableArray{ITypeParameterSymbol}" /> to a string representation.
    /// </summary>
    /// <param name="typeParameterSymbols">The type parameters being converted to a string.</param>
    /// <returns>A string representation of method parameters.</returns>
    private static string GetTypeParameterAsString(ImmutableArray<ITypeParameterSymbol> typeParameterSymbols)
    {
        return string.Join(", ", typeParameterSymbols);
    }

    /// <summary>
    ///     Converts a <see cref="ImmutableArray{IParameterSymbol}" /> to a string representation.
    /// </summary>
    /// <param name="parameterSymbols">The parameters being converted to a string.</param>
    /// <returns>A string representation of method parameters.</returns>
    private static string GetParametersAsString(ImmutableArray<IParameterSymbol> parameterSymbols)
    {
        return string.Join(", ", parameterSymbols.Select(x => $"{x.Type} {x.Name}"));
    }

    /// <summary>
    ///     Builds a list of string representing usings for the service being proxied.
    /// </summary>
    /// <param name="namedTypeSymbol">The <see cref="INamedTypeSymbol" /> representing the service.</param>
    /// <returns>A list of string representing usings for the service being proxied</returns>
    public static IEnumerable<string> GetUsingDirectivesForTypeFile(INamedTypeSymbol namedTypeSymbol)
    {
        var usingDirectivesList = new List<string>();

        // Get a syntax reference (location of the type in source)
        var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();

        if (syntaxReference is null) return usingDirectivesList;

        // Get the CompilationUnitSyntax (root of the file)
        var root = syntaxReference.SyntaxTree.GetCompilationUnitRoot();

        // Retrieve all using directives in the file
        usingDirectivesList.AddRange(root.Usings.Select(usingDirective =>
            $"{usingDirective.NormalizeWhitespace().ToFullString()}{Environment.NewLine}"));

        return usingDirectivesList;
    }

    /// <summary>
    ///     Produces an <see cref="ILookup{TKey,TElement}" />, mapping <see cref="IMethodSymbol" />s to their appropriate
    ///     <see cref="IPropertySymbol" />.
    /// </summary>
    /// <param name="methods">The methods being processed to find auto-properties.</param>
    /// <returns>
    ///     An <see cref="ILookup{TKey,TElement}" />, mapping <see cref="IMethodSymbol" />s to their appropriate
    ///     <see cref="IPropertySymbol" />.
    /// </returns>
    private static ILookup<IPropertySymbol, IMethodSymbol> GetAutoPropertyMethods(
        List<IMethodSymbol> methods
    )
    {
#pragma warning disable RS1024
        var methodPropertyDictionary = new Dictionary<IMethodSymbol, IPropertySymbol>();
#pragma warning restore RS1024

        foreach (var method in methods)
        {
            if (!TryGetAutoProperty(method, out var property)) continue;

            methodPropertyDictionary.Add(method, property!);
        }

#pragma warning disable RS1024
        return methodPropertyDictionary.ToLookup(x => x.Value, x => x.Key);
#pragma warning restore RS1024
    }

    /// <summary>
    ///     Determines whether a particular <see cref="IMethodSymbol" /> is an auto-property.
    /// </summary>
    /// <param name="methodSymbol">The method being checked.</param>
    /// <param name="propertySymbol">The associated <see cref="IPropertySymbol" /> if one exists.</param>
    /// <returns><c>true</c> when the method is a auto-property getter or setter, <c>false</c> if it is not.</returns>
    private static bool TryGetAutoProperty(IMethodSymbol methodSymbol, out IPropertySymbol? propertySymbol)
    {
        propertySymbol = null;

        if (methodSymbol.MethodKind is not (MethodKind.PropertyGet or MethodKind.PropertySet)) return false;

        if (methodSymbol.AssociatedSymbol is not IPropertySymbol property) return false;

        propertySymbol = property;
        return true;
    }

    #endregion
}