using HierarchicalMvvm.Generator.Helpers;
using HierarchicalMvvm.Generator.Metadata;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace HierarchicalMvvm.Generator;

public class CodeGenerator
{
    private readonly ModelGenerationInfo _modelInfo;
    private readonly ImmutableArray<PropertyInfo> _properties;
    private readonly ImmutableArray<PropertyInfo> _allProperties;
    private readonly Dictionary<string, string> _namespaceMapping;
    private readonly SourceProductionContext? _context;

    public CodeGenerator(ModelGenerationInfo modelInfo, Dictionary<string, string> namespaceMapping, SourceProductionContext? context = null)
    {
        _context = context;
        _modelInfo = modelInfo;
        _namespaceMapping = namespaceMapping;
        _properties = PropertyHelper.GetProperties(modelInfo, _namespaceMapping);
        _allProperties = PropertyHelper.GetAllProperties(modelInfo, _namespaceMapping);
    }

    public string Generate()
    {
        var className = _modelInfo.ClassDeclaration.Identifier.ValueText;
        DiagnosticHelper.LogInfo(_context, $"Generating code for {className}");

        try
        {
            var namespaceName = SyntaxHelper.GetNamespace(_modelInfo.ClassDeclaration);
            var targetTypeName = _modelInfo.TargetWrapperType.ToDisplayString();

            var sb = new StringBuilder();
            var hasHierarchicalObjects = HasHierarchicalObjects();

            sb.AppendLine("#nullable enable");

            // Using statements
            GenerateUsingStatements(sb, namespaceName);


            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");


            // Class declaration
            GenerateClassDeclaration(sb, className, targetTypeName);

            // Class body
            sb.AppendLine("    {");

            GenerateFields(sb);

            GenerateProperties(sb);

            // Constructors
            GenerateConstructors(sb, className, targetTypeName);

            // Note: PropertyChanged infrastructure removed because TrackableObject already provides it

            // ToRecord method
            GenerateToRecordMethod(sb);

            // UpdateFrom method
            GenerateUpdateFromMethod(sb);

            // Close class
            sb.AppendLine("    }");

            // Close namespace
            sb.AppendLine("}");

            DiagnosticHelper.LogInfo(_context, $"Successfully generated {className}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            DiagnosticHelper.LogError(_context, $"Error generating {className}: {ex.Message}");
            throw;
        }
    }

    private void GenerateUsingStatements(StringBuilder sb, string namespaceName)
    {
        var modelNamespaces = new HashSet<string>
        {
            "System",
            "System.Linq",
            "System.Collections",
            "System.ComponentModel",
            "System.Collections.Generic",
            "System.Runtime.CompilerServices",
            "System.Collections.ObjectModel",
            "HierarchicalMvvm.Core"
        };



        foreach (var property in _properties)
        {
            var propertyNamespace = property.Type.ContainingNamespace.ToDisplayString();

            if (string.IsNullOrEmpty(propertyNamespace) || modelNamespaces.Contains(propertyNamespace))
                continue;

            modelNamespaces.Add(propertyNamespace);

            if (property.ElementType is INamedTypeSymbol elementType)
            {
                var elementNamespace = elementType.ContainingNamespace.ToDisplayString();

                if (string.IsNullOrEmpty(elementNamespace) || modelNamespaces.Contains(elementNamespace))
                    continue;

                modelNamespaces.Add(elementNamespace);
            }

        }

        foreach (var item in modelNamespaces)
        {
            sb.AppendLine($"using {item};");
        }

        sb.AppendLine();
    }

    private void GenerateClassDeclaration(StringBuilder sb, string className, string targetTypeName)
    {
        var baseTypeName = _modelInfo.BaseType?.ToDisplayString();
        if (baseTypeName is null || baseTypeName == "object")
            baseTypeName = "TrackableObject";  // Changed from DeepObservableObject to TrackableObject

        var interfaces = new List<string> { baseTypeName };

        if (_modelInfo.BaseWrapperType is null)
        {
            interfaces.Add($"IModelWrapper<{targetTypeName}>");
        }

        var modifiers = _modelInfo.IsAbstract ? "public abstract partial" : "public partial";
        sb.AppendLine($"    {modifiers} class {className} : {string.Join(", ", interfaces)}");
    }

    // PropertyChanged infrastructure removed - TrackableObject already provides it

    private void GenerateFields(StringBuilder sb)
    {
        foreach (var property in _properties)
        {
            switch (property.Kind)
            {
                case PropertyKind.Primitive:
                    GenerateStandardField(sb, property);
                    break;
                case PropertyKind.ModelObject:
                    GenerateTrackableField(sb, property);
                    break;
                case PropertyKind.Collection:
                case PropertyKind.ModelCollection:
                    GenerateCollectionField(sb, property);
                    break;
            }
        }
        sb.AppendLine();

    }

    private void GenerateProperties(StringBuilder sb)
    {
        foreach (var property in _properties)
        {
            switch (property.Kind)
            {
                case PropertyKind.Primitive:
                    GenerateStandardProperty(sb, property);
                    break;
                case PropertyKind.ModelObject:
                    GenerateTrackableProperty(sb, property);
                    break;
                case PropertyKind.Collection:
                case PropertyKind.ModelCollection:
                    GenerateCollectionProperty(sb, property);
                    break;
            }
            sb.AppendLine();
        }
    }

    private void GenerateStandardField(StringBuilder sb, PropertyInfo property)
    {
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);
        var defaultValue = GetDefaultValue(property);

        // Backing field
        if (property.IsNullable)
            sb.AppendLine($"        private {property.TypeName}? {fieldName};");
        else
            sb.AppendLine($"        private {property.TypeName} {fieldName}{defaultValue};");
    }

    private void GenerateStandardProperty(StringBuilder sb, PropertyInfo property)
    {
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);
        var defaultValue = GetDefaultValue(property);

        // Public property using SetChildProperty from TrackableObject
        var propertyType = property.IsNullable ? $"{property.TypeName}?" : property.TypeName;
        sb.AppendLine($"        public {propertyType} {property.Name}");
        sb.AppendLine("        {");
        sb.AppendLine($"            get => {fieldName};");
        sb.AppendLine("            set");
        sb.AppendLine("            {");
        sb.AppendLine($"                if ({fieldName} != value)");
        sb.AppendLine("                {");
        sb.AppendLine($"                    {fieldName} = value;");
        sb.AppendLine($"                    base.OnPropertyChangedInternal();");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private void GenerateTrackableField(StringBuilder sb, PropertyInfo property)
    {
        var modelTypeName = property.FullModelTypeName;
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);

        if (property.IsNullable)
            sb.AppendLine($"        private {modelTypeName}? {fieldName};");
        else
            sb.AppendLine($"        private {modelTypeName} {fieldName} = new {modelTypeName}();");
    }

    private void GenerateTrackableProperty(StringBuilder sb, PropertyInfo property)
    {
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);
        var modelTypeName = property.FullModelTypeName;

        if (property.IsNullable)
        {
            sb.AppendLine($"        public {modelTypeName}? {property.Name}");
            sb.AppendLine("        {");
            sb.AppendLine($"            get => {fieldName};");
            sb.AppendLine($"            set => SetChildProperty(ref {fieldName}, value);");  // Changed from SetObjectProperty to SetChildProperty
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine($"        public {modelTypeName} {property.Name}");
            sb.AppendLine("        {");
            sb.AppendLine($"            get => {fieldName};");
            sb.AppendLine($"            set => SetChildProperty(ref {fieldName}, value);");  // Changed from SetObjectProperty to SetChildProperty
            sb.AppendLine("        }");
        }
    }

    private void GenerateCollectionField(StringBuilder sb, PropertyInfo property)
    {
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);

        sb.AppendLine($"        private TrackableCollection<{property.CollectionElementType}> {fieldName} = new TrackableCollection<{property.CollectionElementType}>();");
    }

    private void GenerateCollectionProperty(StringBuilder sb, PropertyInfo property)
    {
        var fieldName = StringHelpers.GetPrivateFieldName(property.Name);
        var elementModelType = property.CollectionElementType; // OPRAVENO: použít CollectionElementType

        sb.AppendLine($"        public TrackableCollection<{elementModelType}> {property.Name}");
        sb.AppendLine("        {");
        sb.AppendLine($"            get => {fieldName};");
        sb.AppendLine($"            set => SetChildProperty(ref {fieldName}, value);");
        sb.AppendLine("        }");
    }



    private void GenerateConstructors(StringBuilder sb, string className, string targetTypeName)
    {
        var collections = _properties.Where(p => p.Kind is PropertyKind.ModelCollection or PropertyKind.Collection).ToArray();

        // Constructor s source parametrem
        if (_modelInfo.IsAbstract)
        {
            sb.AppendLine($"        protected {className}({targetTypeName} source)");
        }
        else if (_modelInfo.IsDerived)
        {
            sb.AppendLine($"        public {className}({targetTypeName} source) : base(source)");
        }
        else
        {
            sb.AppendLine($"        public {className}({targetTypeName} source)");
        }

        sb.AppendLine("        {");

        foreach (var property in _properties)
        {
            switch (property.Kind)
            {
                case PropertyKind.Primitive:
                    sb.AppendLine($"            {property.Name} = source.{property.Name};");
                    break;
                case PropertyKind.ModelObject:
                    sb.AppendLine($"            {property.Name} = source.{property.Name}?.ToModel();");
                    break;
                case PropertyKind.Collection:
                    sb.AppendLine($"            {property.Name} = new TrackableCollection<{property.CollectionElementType}>(source.{property.Name});");
                    break;
                case PropertyKind.ModelCollection:
                    // Pro ModelCollection musíme mapovat elementy na model typy
                    sb.AppendLine($"            {property.Name} = new TrackableCollection<{property.CollectionElementType}>();");
                    sb.AppendLine($"            foreach (var item in source.{property.Name})");
                    sb.AppendLine("            {");
                    sb.AppendLine($"                {property.Name}.Add(item.ToModel());");  // OPRAVENO: používat ToModel
                    sb.AppendLine("            }");
                    break;
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine();

        // Default constructor
        if (_modelInfo.IsAbstract)
        {
            sb.AppendLine($"        protected {className}()");
        }
        else if (_modelInfo.IsDerived)
        {
            sb.AppendLine($"        public {className}() : base()");
        }
        else
        {
            sb.AppendLine($"        public {className}()");
        }

        sb.AppendLine("        {");

        foreach (var collection in collections)
        {
            if (collection.Kind == PropertyKind.ModelCollection)
            {
                sb.AppendLine($"            {collection.Name} = new TrackableCollection<{collection.CollectionElementType}>();");  // Changed from DeepObservableCollection to TrackableCollection
            }
            else
            {
                sb.AppendLine($"            {collection.Name} = new TrackableCollection<{collection.CollectionElementType}>();");
            }
        }

        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private void GenerateToRecordMethod(StringBuilder sb)
    {
        if (_modelInfo.IsAbstract)
        {
            sb.AppendLine($"        public abstract {_modelInfo.TargetWrapperType.ToDisplayString()} ToRecord();");
            sb.AppendLine();
            return;
        }

        var methodSignature = _modelInfo.BaseWrapperType is not null
            ? $"        public override {_modelInfo.BaseWrapperType.ToDisplayString()} ToRecord()"
            : $"        public {_modelInfo.TargetWrapperType.ToDisplayString()} ToRecord()";

        sb.AppendLine(methodSignature);
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {_modelInfo.TargetWrapperType.ToDisplayString()}");
        sb.AppendLine("            {");

        var writableProperties = _allProperties.Where(p => !p.IsReadOnly).ToArray();
        for (int i = 0; i < writableProperties.Length; i++)
        {
            var property = writableProperties[i];
            var comma = i < writableProperties.Length - 1 ? "," : "";
            var conversion = GetToRecordConversion(property);
            sb.AppendLine($"                {property.Name} = {conversion}{comma}");
        }

        sb.AppendLine("            };");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private void GenerateUpdateFromMethod(StringBuilder sb)
    {
        var methodSignature = _modelInfo.IsAbstract
            ? $"        public abstract void UpdateFrom({_modelInfo.TargetWrapperType.ToDisplayString()} data);"
            : _modelInfo.BaseWrapperType is not null
                ? $"        public override void UpdateFrom({_modelInfo.BaseWrapperType.ToDisplayString()} data)"
                : $"        public void UpdateFrom({_modelInfo.TargetWrapperType.ToDisplayString()} data)";

        sb.AppendLine(methodSignature);

        if (_modelInfo.IsAbstract)
        {
            sb.AppendLine();
            return;
        }

        sb.AppendLine("        {");
        sb.AppendLine($"            if (data is {_modelInfo.TargetWrapperType.ToDisplayString()} source)");
        sb.AppendLine("            {");

        foreach (var property in _allProperties)
        {
            GenerateUpdateFromPropertyLogic(sb, property);
        }

        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private void GenerateUpdateFromPropertyLogic(StringBuilder sb, PropertyInfo property)
    {
        switch (property.Kind)
        {
            case PropertyKind.Primitive:
                if (property.TypeName == "string")
                {
                    if (property.IsNullable)
                        sb.AppendLine($"                {property.Name} = source.{property.Name};");
                    else
                        sb.AppendLine($"                {property.Name} = source.{property.Name} ?? string.Empty;");
                }
                else
                {
                    sb.AppendLine($"                {property.Name} = source.{property.Name};");
                }
                break;

            case PropertyKind.ModelObject:
                if (property.IsNullable)
                    sb.AppendLine($"                {property.Name} = source.{property.Name}?.ToModel();");
                else
                    sb.AppendLine($"                {property.Name} = source.{property.Name}.ToModel();");
                break;

            case PropertyKind.ModelCollection:
                sb.AppendLine($"                {property.Name}.Clear();");
                sb.AppendLine($"                foreach (var item in source.{property.Name})");
                sb.AppendLine("                {");
                sb.AppendLine($"                    {property.Name}.Add(item.ToModel());");
                sb.AppendLine("                }");
                break;

            case PropertyKind.Collection:
                sb.AppendLine($"                {property.Name}.Clear();");
                sb.AppendLine($"                foreach (var item in source.{property.Name})");
                sb.AppendLine("                {");
                sb.AppendLine($"                    {property.Name}.Add(item);");
                sb.AppendLine("                }");
                break;
        }
    }

    private bool HasHierarchicalObjects()
    {
        return _properties.Any(p => p.Kind is PropertyKind.ModelObject or PropertyKind.ModelCollection);
    }

    private string GetDefaultValue(PropertyInfo property)
    {
        if (property.TypeName == "string")
            return " = string.Empty";
        else if (!TypeHelper.PrimitiveTypes.Contains(property.TypeName))
            return " = new()";
        return string.Empty;
    }

    private string GetToRecordConversion(PropertyInfo property)
    {
        return property.Kind switch
        {
            PropertyKind.Primitive => property.Name,
            PropertyKind.ModelObject => property.IsNullable ? $"{property.Name}?.ToRecord()" : $"{property.Name}.ToRecord()",
            PropertyKind.ModelCollection => GetCollectionConversion(property),
            PropertyKind.Collection => GetCollectionCast(property),
            _ => property.Name
        };
    }

    private string GetCollectionConversion(PropertyInfo property)
    {

        if (property.Type.Constructors.FirstOrDefault(c => c.Parameters.Length == 1) is IMethodSymbol ctor)
        {
            if (ctor.Parameters.First().Type.ToDisplayString().StartsWith("IEnumerable"))
            {
                return $"new {property.TypeName}({property.Name}.Select(x => x.ToRecord()))";
            }
        }

        if (property.Type.AllInterfaces.Any(i => i.Name.StartsWith("ISet")))
        {
            return $"{property.Name}.Select(x => x.ToRecord()).ToHashSet()";
        }

        return $"{property.Name}.Select(x => x.ToRecord()).ToList()";
    }

    private string GetCollectionCast(PropertyInfo property)
    {

        if (property.Type.Constructors.FirstOrDefault(c => c.Parameters.Length == 1) is IMethodSymbol ctor)
        {
            if (ctor.Parameters.First().Type.ToDisplayString().StartsWith("IEnumerable"))
            {
                return $"new {property.TypeName}({property.Name})";
            }
        }

        if (property.Type.AllInterfaces.Any(i => i.Name.StartsWith("ISet")))
        {
            return $"{property.Name}.ToHashSet()";
        }

        return $"{property.Name}.ToList()";
    }

    private string GetSimpleTypeName(string fullTypeName)
    {
        return fullTypeName.Split('.').Last();
    }

    private string? GetNamespaceFromFullTypeName(string fullTypeName)
    {
        var lastDotIndex = fullTypeName.LastIndexOf('.');
        return lastDotIndex > 0 ? fullTypeName.Substring(0, lastDotIndex) : null;
    }
}