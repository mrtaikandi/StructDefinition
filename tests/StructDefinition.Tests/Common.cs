using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;

namespace StructDefinition.Tests
{
    internal static class Common
    {
        internal const int Indent1 = 4;
        internal const int Indent2 = Indent1 * 2;
        internal const int Indent3 = Indent1 * 3;
        internal static readonly Location IgnoreLocation = Location.None;

        internal static readonly Dictionary<string, string> DefaultAnalyzerOptions = new();

        internal static SyntaxTree? GetGeneratedStructSyntaxTree(this Compilation compilation, string targetStruct)
        {
            return compilation.SyntaxTrees
                .LastOrDefault(s =>  s.GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>().Any(sd => sd.Identifier.ValueText == targetStruct));
        }

        internal static PropertyDeclarationSyntax GetPropertyDeclarationSyntax(SyntaxTree syntaxTree, string targetPropertyName, string targetClass = "Foo")
        {
            return syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single(c => c.Identifier.ValueText == targetClass)
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Single(p => p.Identifier.ValueText == targetPropertyName);
        }

        internal static IPropertySymbol GetSourcePropertySymbol(string propertyName, Compilation compilation, string targetClass = "Foo")
        {
            var syntaxTree = compilation.SyntaxTrees.First();
            var propSyntax = GetPropertyDeclarationSyntax(syntaxTree, propertyName, targetClass);

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            return semanticModel.GetDeclaredSymbol(propSyntax).ShouldNotBeNull();
        }
    }
}