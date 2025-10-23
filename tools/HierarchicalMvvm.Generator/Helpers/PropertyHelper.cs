using HierarchicalMvvm.Generator.Metadata;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace HierarchicalMvvm.Generator.Helpers;

public static class PropertyHelper
{
    public static bool HasBaseTypeWithProperties(INamedTypeSymbol? baseType)
    {
        if (baseType is null)
            return false;

        if (baseType.Name == "Object")
            return false;

        return true;
    }

    public static bool CanCreateBaseTypeProperties(INamedTypeSymbol? baseType, INamedTypeSymbol? baseWrapperType)
    {
        if (baseType is null)
            return false;

        if (baseType.Name == "Object")
            return false;

        if (SymbolEqualityComparer.Default.Equals(baseType, baseWrapperType))
            return false;

        return true;
    }

    public static ImmutableArray<PropertyInfo> GetProperties(ModelGenerationInfo modelInfo, Dictionary<string, string> namespaceMapping)
    {
        var properties = ImmutableArray.CreateBuilder<PropertyInfo>();

        var targetType = modelInfo.TargetWrapperType;

        var baseType = targetType.BaseType;

        while (CanCreateBaseTypeProperties(baseType, modelInfo.BaseWrapperType))
        {
            properties.AddRange(GetProperties(baseType, namespaceMapping, properties.ToImmutable()));

            baseType = baseType?.BaseType;
        }

        properties.AddRange(GetProperties(targetType, namespaceMapping, properties.ToImmutable()));

        return properties.ToImmutable();
    }

    public static ImmutableArray<PropertyInfo> GetCurrentTypeProperties(ModelGenerationInfo modelInfo, Dictionary<string, string> namespaceMapping)
    {
        // Získat pouze vlastnosti specifické pro aktuální třídu (bez vlastností z základní třídy)
        var targetType = modelInfo.TargetWrapperType;
        return GetProperties(targetType, namespaceMapping, ImmutableArray<PropertyInfo>.Empty);
    }

    public static ImmutableArray<PropertyInfo> GetAllProperties(ModelGenerationInfo modelInfo, Dictionary<string, string> namespaceMapping)
    {
        var properties = ImmutableArray.CreateBuilder<PropertyInfo>();

        var targetType = modelInfo.TargetWrapperType;

        var baseType = targetType.BaseType;

        while (HasBaseTypeWithProperties(baseType))
        {
            properties.AddRange(GetProperties(baseType, namespaceMapping, properties.ToImmutable()));

            baseType = baseType?.BaseType;
        }

        properties.AddRange(GetProperties(targetType, namespaceMapping, properties.ToImmutable()));

        return properties.ToImmutable();
    }

    public static ImmutableArray<PropertyInfo> GetProperties(INamedTypeSymbol? targetType, Dictionary<string, string> namespaceMapping, ImmutableArray<PropertyInfo> baseProperties)
    {

        var properties = ImmutableArray.CreateBuilder<PropertyInfo>();

        if (targetType is null)
            return properties.ToImmutable();

        foreach (var member in targetType.GetMembers().OfType<IPropertySymbol>())
        {
            if (IsValidProperty(member))
            {
                var propertyKind = DeterminePropertyKind(member.Type);
                var collectionElementType = GetCollectionElementType(member.Type);

                // Skip duplicit properties
                if (properties.Any(p => p.Name == member.Name) || baseProperties.Any(p => p.Name == member.Name))
                {
                    continue;
                }

                var propertyInfo = new PropertyInfo
                {
                    Name = member.Name,
                    Kind = propertyKind,
                    IsReadOnly = member.IsReadOnly,
                    TypeName = member.Type.ToDisplayString().Replace("?", ""),
                    IsNullable = member.Type.CanBeReferencedByName && member.NullableAnnotation == NullableAnnotation.Annotated,
                    FullModelTypeName = GetFullModelTypeName(member.Type, namespaceMapping, propertyKind, collectionElementType?.ToDisplayString()),
                    CollectionElementType = propertyKind switch
                    {
                        PropertyKind.Collection => collectionElementType?.ToDisplayString(),
                        PropertyKind.ModelCollection => GetFullModelTypeName(member.Type, namespaceMapping, propertyKind, collectionElementType?.ToDisplayString()),
                        _ => default
                    },
                    Type = member.Type as INamedTypeSymbol ?? throw new InvalidOperationException($"Cannot convert {member.Type} to INamedTypeSymbol"),
                    ElementType = collectionElementType as INamedTypeSymbol,
                    Symbol = member
                };

                properties.Add(propertyInfo);
            }
        }

        return properties.ToImmutable();
    }

    private static bool IsValidProperty(IPropertySymbol property)
    {
        return property.Kind == SymbolKind.Property &&
               property.DeclaredAccessibility == Accessibility.Public &&
               !property.IsStatic &&
               !property.IsOverride &&
               property.GetMethod != null;
    }

    private static PropertyKind DeterminePropertyKind(ITypeSymbol type)
    {
        if (TypeHelper.PrimitiveTypes.Contains(type.ToDisplayString().Replace("?", "")))
            return PropertyKind.Primitive;

        // Zkontroluj, zda je to kolekce
        if (TypeHelper.TryGetElementType(type, out var elementType))
        {
            if (elementType != null && ImplementsIModelRecord(elementType))
            {
                return PropertyKind.ModelCollection;
            }
            return PropertyKind.Collection;
        }

        // Zkontroluj, zda je to single model object
        if (ImplementsIModelRecord(type))
        {
            return PropertyKind.ModelObject;
        }

        // Default to primitive
        return PropertyKind.Primitive;
    }

    private static bool ImplementsIModelRecord(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType) return false;

        // Zkontroluj, zda typ implementuje IModelRecord<T>
        return namedType.AllInterfaces.Any(i =>
            i.Name == "IModelRecord" &&
            i.TypeArguments.Length == 1);
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (TypeHelper.TryGetElementType(type, out var elementType))
        {
            return elementType;
        }
        return null;
    }


    private static string? GetFullModelTypeName(ITypeSymbol type, Dictionary<string, string> namespaceMapping, PropertyKind kind, string? collectionElementType)
    {
        switch (kind)
        {
            case PropertyKind.ModelObject:
                if (type is INamedTypeSymbol namedType)
                {
                    return namespaceMapping.TryGetValue(namedType.Name, out var fullName)
                        ? fullName
                        : $"{namedType.Name}Model";
                }
                break;

            case PropertyKind.ModelCollection:
                if (!string.IsNullOrEmpty(collectionElementType))
                {
                    var elementTypeName = collectionElementType!.Split('.').Last();
                    return namespaceMapping.TryGetValue(elementTypeName, out var fullName)
                        ? fullName
                        : $"{elementTypeName}Model";
                }
                break;
        }

        return null;
    }

    public static List<INamedTypeSymbol> GetDependentTypes(ImmutableArray<PropertyInfo> properties)
    {
        var dependencies = new List<INamedTypeSymbol>();

        foreach (var property in properties)
        {
            if (property.Kind == PropertyKind.ModelObject && property.Type != null)
            {
                dependencies.Add(property.Type);
            }
            else if (property.Kind == PropertyKind.ModelCollection && property.ElementType != null)
            {
                dependencies.Add(property.ElementType);
            }
        }

        return dependencies;
    }
}