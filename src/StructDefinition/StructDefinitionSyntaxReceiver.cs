using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StructDefinition
{
    internal class StructDefinitionSyntaxReceiver : ISyntaxReceiver
    {
        public List<AttributeOption> AttributeOptions { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not StructDeclarationSyntax sds)
            {
                return;
            }

            var attribute = sds.AttributeLists
                .SelectMany(a => a.Attributes)
                .SingleOrDefault(a =>
                    a.Name is IdentifierNameSyntax nameSyntax && string.Equals(nameSyntax.Identifier.ValueText, SourceProvider.AttributeName, StringComparison.Ordinal));

            if (attribute?.ArgumentList is null)
            {
                return;
            }

            var ns = syntaxNode.AncestorsAndSelf()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault()
                ?.Name
                .NormalizeWhitespace()
                .ToFullString();

            if (ns is null)
            {
                return;
            }

            var option = new AttributeOption(sds.Identifier.ValueText, ns);

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                switch (argument.NameEquals?.Name.Identifier.ValueText)
                {
                    case null:
                    case nameof(AttributeOption.BaseType):
                        option.BaseType = ((TypeOfExpressionSyntax)argument.Expression).Type.NormalizeWhitespace().ToFullString();
                        break;

                    case nameof(AttributeOption.IsLittleEndian):
                        option.IsLittleEndian = ((LiteralExpressionSyntax)argument.Expression).Token.Value as bool? ?? AttributeOption.IsLittleEndianPropertyDefaultValue;
                        break;

                    case nameof(AttributeOption.HexPadding):
                        option.HexPadding = ((LiteralExpressionSyntax)argument.Expression).Token.Value as int? ?? AttributeOption.HexPaddingPropertyDefaultValue;
                        break;

                    case nameof(AttributeOption.OverrideToString):
                        option.OverrideToString = ((LiteralExpressionSyntax)argument.Expression).Token.Value as bool? ?? AttributeOption.OverrideToStringDefaultValue;
                        break;
                }
            }

            AttributeOptions.Add(option);
        }
    }
}