using DudNet.Generation;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;

namespace DudNet.Tests.Generation;

public class ParserTests
{
   [Fact]
   public void IsPotentialTarget_ShouldReturnFalse_WhenNodeNotClass()
   {
      // Arrange
      const string node = "";

      var syntaxNode = CSharpSyntaxTree.ParseText(node).GetRoot();

      // Act
      var result = Parser.IsPotentialTarget(syntaxNode);

      // Assert
      result.Should().BeFalse();
   }
}