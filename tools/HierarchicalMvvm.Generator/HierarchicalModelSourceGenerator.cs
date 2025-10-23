using HierarchicalMvvm.Generator.Helpers;
using HierarchicalMvvm.Generator.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;


namespace HierarchicalMvvm.Generator;

[Generator]
public class HierarchicalModelSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var modelWrapperCandidates = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "HierarchicalMvvm.Attributes.ModelWrapperAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => SyntaxHelper.GetSemanticTargetForGeneration(ctx)
            )
            .Where(static m => m is not null);

        var compilationAndTypes = context.CompilationProvider
            .Combine(modelWrapperCandidates.Collect());

        context.RegisterSourceOutput(compilationAndTypes, static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ModelGenerationInfo?> modelInfos, SourceProductionContext context)
    {
        if (modelInfos.IsDefaultOrEmpty)
            return;

        if (!modelInfos.Where(m => m is not null).Any())
            return;


        var validModelInfos = modelInfos.Where(m => m is not null).Cast<ModelGenerationInfo>().ToArray();

        if (validModelInfos.Length == 0)
            return;

        DiagnosticHelper.LogInfo(context, $"Processing {validModelInfos.Length} model(s)");

        try
        {
            // Topologický sort podle závislostí
            var sortedModels = DependencyHelper.SortByDependencies(validModelInfos);

            if (DependencyHelper.HasCyclicDependencies(validModelInfos))
            {
                DiagnosticHelper.LogWarning(context, "Cyclic dependencies detected, using original order");
                sortedModels = validModelInfos.ToList();
            }

            var namespaceMapping = BuildNamespaceMapping(sortedModels);

            foreach (var modelInfo in sortedModels)
            {
                try
                {
                    var generator = new CodeGenerator(modelInfo, namespaceMapping, context);
                    var sourceCode = generator.Generate();

                    var className = modelInfo.ClassDeclaration.Identifier.ValueText;
                    var classNamespace = SyntaxHelper.GetNamespace(modelInfo.ClassDeclaration);

                    if (!string.IsNullOrEmpty(classNamespace))
                        context.AddSource($"{classNamespace.Replace(".", "\\")}\\{className}.g.cs", sourceCode);
                    else
                        context.AddSource($"{className}.g.cs", sourceCode);

                    DiagnosticHelper.LogInfo(context, $"Successfully generated {className}");
                }
                catch (Exception ex)
                {
                    var className = modelInfo.ClassDeclaration.Identifier.ValueText;
                    DiagnosticHelper.LogError(context, $"Failed to generate {className}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            DiagnosticHelper.LogError(context, $"Error during generation: {ex.Message}");
        }
    }

    private static Dictionary<string, string> BuildNamespaceMapping(List<ModelGenerationInfo> modelInfos)
    {
        var mapping = new Dictionary<string, string>();

        foreach (var modelInfo in modelInfos)
        {
            var modelClassName = modelInfo.ClassDeclaration.Identifier.ValueText;
            var modelNamespace = SyntaxHelper.GetNamespace(modelInfo.ClassDeclaration);

            var targetTypeSimpleName = modelInfo.TargetWrapperType.Name;

            var fullModelTypeName = string.IsNullOrEmpty(modelNamespace)
                ? modelClassName
                : $"{modelNamespace}.{modelClassName}";

            mapping[targetTypeSimpleName] = fullModelTypeName;
        }

        return mapping;
    }

}