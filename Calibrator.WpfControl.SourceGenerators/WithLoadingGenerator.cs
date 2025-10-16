using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Calibrator.WpfControl.SourceGenerators;

/// <summary>
/// Source generator for [WithLoading] attribute.
/// Generates partial method bodies that automatically manage IsLoading state.
/// 
/// The partial method must be declared without a body, and there must be a corresponding
/// Core method that contains the actual business logic.
/// </summary>
[Generator]
public class WithLoadingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes that have partial methods with [WithLoading] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.Modifiers.Any(m => m.Text == "partial");
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check if class inherits from BaseViewModel
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null || !InheritsFromBaseViewModel(classSymbol))
        {
            return null;
        }

        // Check if any method has [WithLoading] attribute and is partial
        foreach (var member in classDeclaration.Members)
        {
            if (member is MethodDeclarationSyntax method && method.Modifiers.Any(m => m.Text == "partial"))
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
                if (methodSymbol?.GetAttributes().Any(a =>
                    a.AttributeClass?.Name == "WithLoadingAttribute" &&
                    a.AttributeClass?.ContainingNamespace?.ToDisplayString() == "Calibrator.WpfControl.Attributes") == true)
                {
                    return classDeclaration;
                }
            }
        }

        return null;
    }

    private static bool InheritsFromBaseViewModel(INamedTypeSymbol classSymbol)
    {
        var baseType = classSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == "BaseViewModel" &&
                baseType.ContainingNamespace?.ToDisplayString() == "Calibrator.WpfControl.Abstract")
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static void Execute(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (!classes.Any())
        {
            return;
        }

        foreach (var classDeclaration in classes)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol == null)
            {
                continue;
            }

            var methods = new List<MethodData>();

            // Collect all partial methods with [WithLoading] attribute
            foreach (var member in classDeclaration.Members)
            {
                if (member is MethodDeclarationSyntax method && method.Modifiers.Any(m => m.Text == "partial"))
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
                    if (methodSymbol == null)
                    {
                        continue;
                    }

                    var withLoadingAttr = methodSymbol.GetAttributes()
                        .FirstOrDefault(a =>
                            a.AttributeClass?.Name == "WithLoadingAttribute" &&
                            a.AttributeClass?.ContainingNamespace?.ToDisplayString() == "Calibrator.WpfControl.Attributes");

                    if (withLoadingAttr != null)
                    {
                        // Get explicit core method name from attribute constructor parameter
                        string? explicitCoreMethodName = null;
                        if (withLoadingAttr.ConstructorArguments.Length > 0)
                        {
                            explicitCoreMethodName = withLoadingAttr.ConstructorArguments[0].Value?.ToString();
                        }

                        // Determine core method name
                        string coreMethodName;
                        if (!string.IsNullOrEmpty(explicitCoreMethodName))
                        {
                            // Use explicit name and add Async suffix if needed
                            coreMethodName = methodSymbol.IsAsync || methodSymbol.ReturnType.Name.Contains("Task")
                                ? (explicitCoreMethodName.EndsWith("Async", StringComparison.Ordinal) 
                                    ? explicitCoreMethodName 
                                    : explicitCoreMethodName + "Async")
                                : explicitCoreMethodName;
                        }
                        else
                        {
                            // Auto-detect: add "Core" or "CoreAsync" suffix
                            coreMethodName = GetCoreMethodName(methodSymbol.Name);
                        }

                        var isAsync = methodSymbol.IsAsync || methodSymbol.ReturnType.Name.Contains("Task");
                        var returnType = methodSymbol.ReturnType.ToDisplayString();
                        var modifiers = string.Join(" ", method.Modifiers.Select(m => m.Text));

                        methods.Add(new MethodData
                        {
                            PartialMethodName = methodSymbol.Name,
                            CoreMethodName = coreMethodName,
                            ReturnType = returnType,
                            IsAsync = isAsync,
                            Modifiers = modifiers,
                            Parameters = methodSymbol.Parameters.Select(p => new ParameterData
                            {
                                Type = p.Type.ToDisplayString(),
                                Name = p.Name
                            }).ToList()
                        });
                    }
                }
            }

            if (methods.Count > 0)
            {
                var source = GenerateSource(classSymbol, methods);
                context.AddSource($"{classSymbol.Name}.WithLoading.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        }
    }

    private static string GetCoreMethodName(string partialMethodName)
    {
        // Add "CoreAsync" suffix for async methods, or "Core" for sync methods
        if (partialMethodName.EndsWith("Async", StringComparison.Ordinal))
        {
            return partialMethodName.Substring(0, partialMethodName.Length - 5) + "CoreAsync";
        }

        return partialMethodName + "Core";
    }

    private static string GenerateSource(INamedTypeSymbol classSymbol, List<MethodData> methods)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        var namespaceName = classSymbol.ContainingNamespace?.ToDisplayString();
        if (!string.IsNullOrEmpty(namespaceName))
        {
            sb.AppendLine($"namespace {namespaceName};");
            sb.AppendLine();
        }

        sb.AppendLine($"public partial class {classSymbol.Name}");
        sb.AppendLine("{");

        foreach (var method in methods)
        {
            GeneratePartialMethodBody(sb, method);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GeneratePartialMethodBody(StringBuilder sb, MethodData method)
    {
        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));
        var parameterNames = string.Join(", ", method.Parameters.Select(p => p.Name));

        sb.AppendLine();
        sb.AppendLine($"    /// <summary>");
        sb.AppendLine($"    /// Generated body for {method.PartialMethodName} with automatic IsLoading state management.");
        sb.AppendLine($"    /// Calls {method.CoreMethodName} with loading state wrapping.");
        sb.AppendLine($"    /// Ensures only one WithLoading task runs at a time.");
        sb.AppendLine($"    /// </summary>");

        if (method.IsAsync)
        {
            // Async method body
            sb.Append($"    {method.Modifiers} async {method.ReturnType} {method.PartialMethodName}(");
            sb.Append(parameters);
            sb.AppendLine(")");
            sb.AppendLine("    {");

            // IsLoading check for concurrency protection
            if (method.ReturnType.Contains("Task<"))
            {
                // Task<T> - need to return default value
                var returnTypeGeneric = ExtractGenericType(method.ReturnType);
                sb.AppendLine($"        if (IsLoading) return default({returnTypeGeneric})!;");
            }
            else
            {
                // Task (void) - just return
                sb.AppendLine("        if (IsLoading) return;");
            }

            sb.AppendLine("        IsLoading = true;");
            sb.AppendLine("        try");
            sb.AppendLine("        {");

            if (method.ReturnType != "System.Threading.Tasks.Task" && method.ReturnType.Contains("Task"))
            {
                // Task<T>
                sb.Append($"            return await {method.CoreMethodName}(");
                sb.Append(parameterNames);
                sb.AppendLine(").ConfigureAwait(false);");
            }
            else
            {
                // Task (void)
                sb.Append($"            await {method.CoreMethodName}(");
                sb.Append(parameterNames);
                sb.AppendLine(").ConfigureAwait(false);");
            }

            sb.AppendLine("        }");
            sb.AppendLine("        finally");
            sb.AppendLine("        {");
            sb.AppendLine("            IsLoading = false;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
        else
        {
            // Synchronous method body
            sb.Append($"    {method.Modifiers} {method.ReturnType} {method.PartialMethodName}(");
            sb.Append(parameters);
            sb.AppendLine(")");
            sb.AppendLine("    {");

            // IsLoading check
            if (method.ReturnType != "void")
            {
                sb.AppendLine($"        if (IsLoading) return default({method.ReturnType})!;");
            }
            else
            {
                sb.AppendLine("        if (IsLoading) return;");
            }

            sb.AppendLine("        IsLoading = true;");
            sb.AppendLine("        try");
            sb.AppendLine("        {");

            if (method.ReturnType != "void")
            {
                sb.Append($"            return {method.CoreMethodName}(");
            }
            else
            {
                sb.Append($"            {method.CoreMethodName}(");
            }
            sb.Append(parameterNames);
            sb.AppendLine(");");

            sb.AppendLine("        }");
            sb.AppendLine("        finally");
            sb.AppendLine("        {");
            sb.AppendLine("            IsLoading = false;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
        }
    }

    private static string ExtractGenericType(string taskType)
    {
        // Extract T from Task<T>
        var start = taskType.IndexOf('<');
        var end = taskType.LastIndexOf('>');
        if (start >= 0 && end > start)
        {
            return taskType.Substring(start + 1, end - start - 1);
        }
        return "object";
    }

    private sealed class MethodData
    {
        public string PartialMethodName { get; set; } = string.Empty;
        public string CoreMethodName { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public bool IsAsync { get; set; }
        public string Modifiers { get; set; } = string.Empty;
        public List<ParameterData> Parameters { get; set; } = new();
    }

    private sealed class ParameterData
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
