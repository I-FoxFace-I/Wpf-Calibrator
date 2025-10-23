using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HierarchicalMvvm.Generator.Metadata;

public class ModelGenerationInfo
{
    public bool IsDerived { get; set; }
    public bool IsAbstract { get; set; }
    public INamedTypeSymbol? BaseType { get; set; }
    public INamedTypeSymbol? BaseWrapperType { get; set; }
    public INamedTypeSymbol GeneratedType { get; set; } = null!;
    public INamedTypeSymbol TargetWrapperType { get; set; } = null!;
    public SemanticModel SemanticModel { get; set; } = null!;
    public ClassDeclarationSyntax ClassDeclaration { get; set; } = null!;
    public ClassDeclarationSyntax TargetClassDeclaration { get; set; } = null!;
}
