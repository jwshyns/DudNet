using DudNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DudNet.Generation;

/// <summary>
///     Functionality for parsing for appropriate source generation targets.
/// </summary>
internal sealed class Parser
{
	/// <summary>
	///     Determines whether a particular <see cref="SyntaxNode" /> is potential target for proxy service generation.
	/// </summary>
	/// <param name="node">The node to evaluate.</param>
	/// <returns><c>True</c> if the node is a potential target, <c>False</c> if not.</returns>
	public static bool IsPotentialTarget(SyntaxNode node)
	{
		return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax &&
		       HasProxyServiceAttribute(classDeclarationSyntax);
	}

	/// <summary>
	///     Checks whether a provided <see cref="MemberDeclarationSyntax" /> is marked with
	///     <see cref="ProxyServiceAttribute" />.
	/// </summary>
	/// <param name="syntax">The syntax to be evaluated</param>
	/// <returns><c>True</c> if the provided member has the attribute, <c>False</c> if it does not.</returns>
	private static bool HasProxyServiceAttribute(MemberDeclarationSyntax syntax)
	{
		foreach (var attributeList in syntax.AttributeLists)
		{
			foreach (var attribute in attributeList.Attributes)
			{
				var attributeName = attribute.Name.ToFullString().Trim();

				switch (attributeName)
				{
					// Check for the short name, like "ProxyService"
					case "ProxyService" or "ProxyServiceAttribute":
					// If the namespace is used, it might be prefixed to the attribute name
					case "DudNet.Attributes.ProxyService" or "DudNet.Attributes.ProxyServiceAttribute":
						return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	///     Gets the <see cref="INamespaceSymbol" /> for a provided <see cref="IGeneratorSyntaxContextWrapper" />.
	/// </summary>
	/// <param name="context">The provided context wrapper.</param>
	/// <returns>The appropriate <see cref="INamespaceSymbol" /> for the provided context.</returns>
	public static INamedTypeSymbol? GetSemanticTargetForGeneration(IGeneratorSyntaxContextWrapper context)
	{
		var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
		return context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
	}
}