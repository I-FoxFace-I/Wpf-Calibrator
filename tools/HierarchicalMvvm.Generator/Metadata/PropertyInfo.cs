using Microsoft.CodeAnalysis;

namespace HierarchicalMvvm.Generator.Metadata;

public class PropertyInfo
{
    public bool IsReadOnly { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? CollectionElementType { get; set; }
    public string? FullModelTypeName { get; set; }
    public string? Namespace { get; set; }
    public PropertyKind Kind { get; set; }
    public INamedTypeSymbol Type { get; set; } = null!;
    public INamedTypeSymbol? ElementType { get; set; }

    /// <summary>
    /// Původní IPropertySymbol pro přístup k metadata
    /// </summary>
    public IPropertySymbol Symbol { get; set; } = null!;
}
