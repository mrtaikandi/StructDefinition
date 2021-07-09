﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using StructDefinition.Tests.Extensions;

namespace StructDefinition.Tests.Infrastructure
{
    internal static class CSharpGenerator
    {
        internal static (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) GetOutputCompilation(
            string source,
            bool assertCompilation = false,
            IDictionary<string, string>? analyzerConfigOptions = null,
            NullableContextOptions nullableContextOptions = NullableContextOptions.Disable,
            LanguageVersion languageVersion = LanguageVersion.CSharp9) =>
            GetOutputCompilation(
                new[] { source },
                assertCompilation,
                analyzerConfigOptions,
                nullableContextOptions,
                languageVersion);
        
        internal static (Compilation compilation, ImmutableArray<Diagnostic> diagnostics) GetOutputCompilation(
            IEnumerable<string> sources, 
            bool assertCompilation = false, 
            IDictionary<string, string>? analyzerConfigOptions = null,
            NullableContextOptions nullableContextOptions = NullableContextOptions.Disable, 
            LanguageVersion languageVersion = LanguageVersion.CSharp9)
        {
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();
            
            var compilation = CSharpCompilation.Create(
                $"{typeof(CSharpGenerator).Assembly.GetName().Name}.Dynamic",
                sources.Select((source, index) => CSharpSyntaxTree.ParseText(source, path: $"Test{index:00}.g.cs", options: new CSharpParseOptions(languageVersion))),
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: nullableContextOptions));
            
            if (assertCompilation)
            {
                // NB: fail tests when the injected program isn't valid _before_ running generators
                compilation.GetDiagnostics().ShouldBeSuccessful();
            }
            
            var driver = CSharpGeneratorDriver.Create(
                new[] { new StructDefinitionGenerator() },
                optionsProvider: new TestAnalyzerConfigOptionsProvider(analyzerConfigOptions),
                parseOptions: new CSharpParseOptions(languageVersion)
            );

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            
            generateDiagnostics.ShouldBeSuccessful(ignoreDiagnosticsIds: new[] { "SD" });
            outputCompilation.GetDiagnostics().ShouldBeSuccessful(outputCompilation);

            return (outputCompilation, generateDiagnostics);
        }
    }
}