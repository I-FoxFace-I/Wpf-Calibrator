using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Calibrator.WpfControl.SourceGenerators;

/// <summary>
/// Source generator that creates relay commands with automatic loading state management.
/// </summary>
[Generator]
public class RelayCommandWithLoadingGenerator : IIncrementalGenerator
{
    private const string AttributeName = "Calibrator.WpfControl.Attributes.RelayCommandWithLoadingAttribute";
    private const string BaseViewModelName = "Calibrator.WpfControl.Abstract.BaseViewModel";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter methods with the attribute
        var methodDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateMethod(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndMethods = context.CompilationProvider.Combine(methodDeclarations.Collect());

        // Generate code
        context.RegisterSourceOutput(compilationAndMethods,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateMethod(SyntaxNode node)
    {
        return node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private static MethodDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Check if method has our attribute
        foreach (var attributeList in methodDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                if (symbol is IMethodSymbol attrSymbol)
                {
                    var attrClass = attrSymbol.ContainingType.ToDisplayString();
                    if (attrClass == AttributeName)
                    {
                        return methodDeclaration;
                    }
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
    {
        if (methods.IsDefaultOrEmpty)
        {
            return;
        }

        // Group methods by containing class
        var methodsByClass = new Dictionary<INamedTypeSymbol, List<MethodDeclarationSyntax>>(SymbolEqualityComparer.Default);

        foreach (var method in methods)
        {
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol?.ContainingType is null)
            {
                continue;
            }

            var containingType = methodSymbol.ContainingType;

            // Check if class is partial
            if (!IsPartialClass(containingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "RCWL001",
                        "Class must be partial",
                        "Class '{0}' must be declared as partial to use [RelayCommandWithLoading]",
                        "RelayCommandWithLoading",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    method.GetLocation(),
                    containingType.Name));
                continue;
            }

            // Check if class inherits from BaseViewModel
            if (!InheritsFromBaseViewModel(containingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "RCWL002",
                        "Must inherit from BaseViewModel",
                        "Class '{0}' must inherit from BaseViewModel to use [RelayCommandWithLoading]",
                        "RelayCommandWithLoading",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    method.GetLocation(),
                    containingType.Name));
                continue;
            }

            if (!methodsByClass.ContainsKey(containingType))
            {
                methodsByClass[containingType] = new List<MethodDeclarationSyntax>();
            }

            methodsByClass[containingType].Add(method);
        }

        // Generate code for each class
        foreach (var kvp in methodsByClass)
        {
            var typeSymbol = kvp.Key;
            var classMethods = kvp.Value;

            var source = GenerateCodeForClass(compilation, typeSymbol, classMethods);
            var fileName = $"{typeSymbol.Name}.RelayCommandWithLoading.g.cs";
            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static bool IsPartialClass(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
    }

    private static bool InheritsFromBaseViewModel(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == BaseViewModelName)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static string GenerateCodeForClass(Compilation compilation, INamedTypeSymbol typeSymbol, List<MethodDeclarationSyntax> methods)
    {
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using System.Windows.Input;");
        sb.AppendLine("using CommunityToolkit.Mvvm.Input;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    partial class {className}");
        sb.AppendLine("    {");

        foreach (var method in methods)
        {
            var semanticModel = compilation.GetSemanticModel(method.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            if (methodSymbol == null)
            {
                continue;
            }

            var attributeData = GetAttributeData(methodSymbol);
            GenerateCommandForMethod(sb, methodSymbol, attributeData);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static CommandAttributeData GetAttributeData(IMethodSymbol methodSymbol)
    {
        var attribute = methodSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeName);

        var data = new CommandAttributeData();

        if (attribute != null)
        {
            foreach (var namedArg in attribute.NamedArguments)
            {
                switch (namedArg.Key)
                {
                    case "CanExecute":
                        data.CanExecute = namedArg.Value.Value?.ToString();
                        break;
                    case "AllowConcurrentExecutions":
                        data.AllowConcurrentExecutions = (bool)(namedArg.Value.Value ?? false);
                        break;
                    case "IncludeCancelCommand":
                        data.IncludeCancelCommand = (bool)(namedArg.Value.Value ?? false);
                        break;
                    case "FlowExceptionsToTaskScheduler":
                        data.FlowExceptionsToTaskScheduler = (bool)(namedArg.Value.Value ?? false);
                        break;
                }
            }
        }

        return data;
    }

    private static void GenerateCommandForMethod(StringBuilder sb, IMethodSymbol methodSymbol, CommandAttributeData attributeData)
    {
        var methodName = methodSymbol.Name;
        var commandName = GetCommandName(methodName);
        var wrapperMethodName = GetWrapperMethodName(methodName);
        var fieldName = GetFieldName(commandName);

        var returnType = methodSymbol.ReturnType;
        var isAsync = returnType.Name == "Task";
        var hasReturnValue = returnType is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0;
        var returnValueType = hasReturnValue ? ((INamedTypeSymbol)returnType).TypeArguments[0].ToDisplayString() : null;

        // Get parameters (excluding CancellationToken)
        var parameters = methodSymbol.Parameters
            .Where(p => p.Type.Name != "CancellationToken")
            .ToList();

        var hasCancellationToken = methodSymbol.Parameters.Any(p => p.Type.Name == "CancellationToken");

        // Determine command type
        string commandInterfaceType;
        string commandImplementationType;
        string parameterType = string.Empty;

        if (parameters.Count == 0)
        {
            commandInterfaceType = "IAsyncRelayCommand";
            commandImplementationType = "AsyncRelayCommand";
        }
        else if (parameters.Count == 1)
        {
            parameterType = parameters[0].Type.ToDisplayString();
            commandInterfaceType = $"IAsyncRelayCommand<{parameterType}>";
            commandImplementationType = $"AsyncRelayCommand<{parameterType}>";
        }
        else
        {
            // Multiple parameters - use tuple
            var tupleTypes = string.Join(", ", parameters.Select(p => p.Type.ToDisplayString()));
            parameterType = $"({tupleTypes})";
            commandInterfaceType = $"IAsyncRelayCommand<{parameterType}>";
            commandImplementationType = $"AsyncRelayCommand<{parameterType}>";
        }

        // Generate wrapper method
        GenerateWrapperMethod(sb, methodSymbol, wrapperMethodName, attributeData, parameters, hasCancellationToken, hasReturnValue, returnValueType);

        // Generate command property
        sb.AppendLine();
        sb.AppendLine($"        private {commandInterfaceType}? {fieldName};");
        sb.AppendLine();
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// Gets the command for {methodName} with automatic loading state management.");
        sb.AppendLine($"        /// </summary>");
        sb.Append($"        public {commandInterfaceType} {commandName} => {fieldName} ??= new {commandImplementationType}({wrapperMethodName}");

        if (!string.IsNullOrEmpty(attributeData.CanExecute))
        {
            sb.Append($", {attributeData.CanExecute}");
        }

        sb.AppendLine(");");
        sb.AppendLine();
    }

    private static void GenerateWrapperMethod(
        StringBuilder sb,
        IMethodSymbol methodSymbol,
        string wrapperMethodName,
        CommandAttributeData attributeData,
        List<IParameterSymbol> parameters,
        bool hasCancellationToken,
        bool hasReturnValue,
        string? returnValueType)
    {
        var methodName = methodSymbol.Name;
        var returnTypeString = hasReturnValue ? $"Task<{returnValueType}>" : "Task";

        sb.AppendLine();
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// Wrapper method for {methodName} with loading state management.");
        sb.AppendLine($"        /// </summary>");

        // Method signature
        sb.Append($"        private async {returnTypeString} {wrapperMethodName}(");

        if (parameters.Count == 1)
        {
            sb.Append($"{parameters[0].Type.ToDisplayString()} {parameters[0].Name}");
        }
        else if (parameters.Count > 1)
        {
            // Tuple parameter
            var tupleTypes = string.Join(", ", parameters.Select(p => p.Type.ToDisplayString()));
            sb.Append($"({tupleTypes}) parameters");
        }

        if (hasCancellationToken && attributeData.IncludeCancelCommand)
        {
            if (parameters.Count > 0)
            {
                sb.Append(", ");
            }
            sb.Append("CancellationToken cancellationToken");
        }

        sb.AppendLine(")");
        sb.AppendLine("        {");

        // Add concurrent execution check
        if (!attributeData.AllowConcurrentExecutions)
        {
            if (hasReturnValue)
            {
                sb.AppendLine($"            if (IsLoading) return default({returnValueType})!;");
            }
            else
            {
                sb.AppendLine("            if (IsLoading) return;");
            }
        }

        sb.AppendLine("            IsLoading = true;");
        sb.AppendLine("            try");
        sb.AppendLine("            {");

        // Call original method
        sb.Append("                ");
        if (hasReturnValue)
        {
            sb.Append("return ");
        }

        sb.Append($"await {methodName}(");

        // Pass parameters
        if (parameters.Count == 1)
        {
            sb.Append(parameters[0].Name);
        }
        else if (parameters.Count > 1)
        {
            // Destructure tuple
            for (int i = 0; i < parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append($"parameters.Item{i + 1}");
            }
        }

        if (hasCancellationToken)
        {
            if (parameters.Count > 0)
            {
                sb.Append(", ");
            }
            if (attributeData.IncludeCancelCommand)
            {
                sb.Append("cancellationToken");
            }
            else
            {
                sb.Append("CancellationToken.None");
            }
        }

        sb.AppendLine(").ConfigureAwait(false);");
        sb.AppendLine("            }");
        sb.AppendLine("            finally");
        sb.AppendLine("            {");
        sb.AppendLine("                IsLoading = false;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static string GetCommandName(string methodName)
    {
        // Remove "Async" suffix if present
        var name = methodName.EndsWith("Async") ? methodName.Substring(0, methodName.Length - 5) : methodName;
        return name + "Command";
    }

    private static string GetWrapperMethodName(string methodName)
    {
        var baseName = methodName.EndsWith("Async") ? methodName.Substring(0, methodName.Length - 5) : methodName;
        return baseName + "WithLoadingWrapperAsync";
    }

    private static string GetFieldName(string commandName)
    {
        return "_" + char.ToLowerInvariant(commandName[0]) + commandName.Substring(1);
    }

    private class CommandAttributeData
    {
        public string? CanExecute { get; set; }
        public bool AllowConcurrentExecutions { get; set; }
        public bool IncludeCancelCommand { get; set; }
        public bool FlowExceptionsToTaskScheduler { get; set; }
    }
}

