using Microsoft.CodeAnalysis;

namespace StructDefinition
{
    [Generator]
    public class StructDefinitionGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var (hintName, source) = SourceProvider.AttributeSource();
            context.AddSource(hintName, source);

            (hintName, source) = SourceProvider.InterfaceSource();
            context.AddSource(hintName, source);

            if (context.SyntaxReceiver is not StructDefinitionSyntaxReceiver syntaxReceiver)
            {
                return;
            }

            foreach (var option in syntaxReceiver.AttributeOptions)
            {
                (hintName, source) = SourceProvider.Source(option);
                context.AddSource(hintName, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new StructDefinitionSyntaxReceiver());
    }
}
