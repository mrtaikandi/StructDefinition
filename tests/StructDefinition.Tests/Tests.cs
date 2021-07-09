using System.Linq;
using StructDefinition.Tests.Extensions;
using StructDefinition.Tests.Infrastructure;
using Xunit;
using static StructDefinition.Tests.Common;

namespace StructDefinition.Tests
{
    public class Tests
    {
        [Fact]
        public void VerifyStructDefinitionAttribute()
        {
            // Arrange
            const string source = "";
            var expectedAttribute = @"
#nullable enable
namespace StructDefinition
{
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class StructDefinitionAttribute: Attribute
    {
        public StructDefinitionAttribute(Type baseType)
        {
            BaseType = baseType;
        }

        public Type BaseType { get; }

        public bool IsLittleEndian { get; set; } = true;

        public int HexPadding { get; set; } = -1;

        public bool OverrideToString { get; set; } = true;
    }
}
".Trim();

            // Act
            var (compilation, diagnostics) = CSharpGenerator.GetOutputCompilation(source, analyzerConfigOptions: DefaultAnalyzerOptions);

            // Assert
            diagnostics.ShouldBeSuccessful();
            compilation.SyntaxTrees.ShouldContainSource(SourceProvider.AttributeFullName, expectedAttribute);
        }

        [Fact]
        public void When_StructIsReadonly_Should_DeclareInternalValueAsReadOnly()
        {
            // Arrange
            const string? source = @"
namespace Test
{
    [StructDefinition.StructDefinition(typeof(long))]
    public readonly partial struct TestStruct { }
}
";

            const string expected = "private readonly long Value;";

            // Act
            var (compilation, diagnostics) = CSharpGenerator.GetOutputCompilation(source, analyzerConfigOptions: DefaultAnalyzerOptions);

            // Assert
            diagnostics.ShouldBeSuccessful();
            compilation.GetGeneratedStructSyntaxTree("TestStruct").ShouldContainPartialSource(expected);
        }
    }
}