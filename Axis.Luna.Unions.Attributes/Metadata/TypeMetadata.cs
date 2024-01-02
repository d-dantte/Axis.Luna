using System.Collections.Immutable;
using System.Text;

namespace Axis.Luna.Unions.Attributes.Metadata
{
    public interface ITypeMetadata
    {
        TypeKind Kind { get; }

        string FullName();

        string SimpleName();
    }

    public interface ITypeMetadata<TSelf>: ITypeMetadata
    where TSelf : ITypeMetadata<TSelf>
    {
        public abstract static TSelf Of(Type type);
    }

    public interface IProperTypeMetadata: ITypeMetadata
    {
        public AccessModifier AccessModifier { get; }

        public string? Namespace { get; }

        public string Name { get; }

        public ImmutableArray<ITypeMetadata> GenericArgs { get; }

        public int Arity { get; }

        public NestingInfo NestingInfo { get; }
    }

    public record NestingInfo
    {
        public IProperTypeMetadata? Container { get; set; }
        public IProperTypeMetadata? NestedType { get; set; }
    }

    public readonly struct ClassMetadata :
        IProperTypeMetadata,
        ITypeMetadata<ClassMetadata>
    {
        public TypeKind Kind => TypeKind.Class;

        public AccessModifier AccessModifier { get; }

        public string? Namespace { get; }

        public string Name { get; }

        public ImmutableArray<ITypeMetadata> GenericArgs { get; }

        public int Arity => GenericArgs.Length;

        public NestingInfo NestingInfo { get; }


        public ClassMetadata(
            AccessModifier accessModifier,
            string? @namespace,
            string name,
            params ITypeMetadata[] genericArgs)
        {
            AccessModifier = accessModifier;
            Namespace = @namespace;
            Name = name;
            NestingInfo = new NestingInfo();
            GenericArgs = genericArgs
                .ThrowIfAny(
                    metadata => metadata is null,
                    _ => new InvalidOperationException("Invalid metadata: null"))
                .ToImmutableArray();
        }

        public static ClassMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.Class != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.Class}'");

            return new ClassMetadata(
                type.GetAccessModifier(),
                type.Namespace,
                type.Name,
                type.GetGenericArgumentTypeMetadata());
        }

        public string FullName()
        {
            return new StringBuilder()
                .Append(Namespace)
                .Append(string.IsNullOrWhiteSpace(Namespace) ? "" : ".")
                .Append(SimpleName())
                .ToString();
        }

        public string SimpleName()
        {
            var sbuilder = new StringBuilder(Name);

            if (GenericArgs.Length > 0)
            {
                sbuilder.Append('<');
                for (int cnt = 0; cnt < GenericArgs.Length; cnt++)
                {
                    if (cnt > 0)
                        sbuilder.Append(", ");

                    sbuilder.Append(GenericArgs[cnt].FullName());
                }
                sbuilder.Append('>');
            }

            return sbuilder.ToString();
        }
    }

    public readonly struct StructMetadata :
        IProperTypeMetadata,
        ITypeMetadata<StructMetadata>
    {
        public TypeKind Kind => TypeKind.Struct;

        public AccessModifier AccessModifier { get; }

        public string? Namespace { get; }

        public string Name { get; }

        public ImmutableArray<ITypeMetadata> GenericArgs { get; }

        public int Arity => GenericArgs.Length;

        public NestingInfo NestingInfo { get; }


        public StructMetadata(
            AccessModifier accessModifier,
            string? @namespace,
            string name,
            params ITypeMetadata[] genericArgs)
        {
            AccessModifier = accessModifier;
            Namespace = @namespace;
            Name = name;
            NestingInfo = new NestingInfo();
            GenericArgs = genericArgs
                .ThrowIfAny(
                    metadata => metadata is null,
                    _ => new InvalidOperationException("Inalid metadata: null"))
                .ToImmutableArray();
        }

        public static StructMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.Struct != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.Struct}'");

            return new StructMetadata(
                type.GetAccessModifier(),
                type.Namespace,
                type.Name,
                type.GetGenericArgumentTypeMetadata());
        }

        public string FullName()
        {
            return new StringBuilder()
                .Append(Namespace)
                .Append(string.IsNullOrWhiteSpace(Namespace) ? "" : ".")
                .Append(SimpleName())
                .ToString();
        }

        public string SimpleName()
        {
            var sbuilder = new StringBuilder(Name);

            if (GenericArgs.Length > 0)
            {
                sbuilder.Append('<');
                for (int cnt = 0; cnt < GenericArgs.Length; cnt++)
                {
                    if (cnt > 0)
                        sbuilder.Append(", ");

                    sbuilder.Append(GenericArgs[cnt].FullName());
                }
                sbuilder.Append('>');
            }

            return sbuilder.ToString();
        }
    }

    public readonly struct InterfaceMetadata :
        IProperTypeMetadata,
        ITypeMetadata<InterfaceMetadata>
    {
        public TypeKind Kind => TypeKind.Interface;

        public AccessModifier AccessModifier { get; }

        public string? Namespace { get; }

        public string Name { get; }

        public ImmutableArray<ITypeMetadata> GenericArgs { get; }

        public int Arity => GenericArgs.Length;

        public NestingInfo NestingInfo { get; }

        public InterfaceMetadata(
            AccessModifier accessModifier,
            string? @namespace,
            string name,
            params ITypeMetadata[] genericArgs)
        {
            AccessModifier = accessModifier;
            Namespace = @namespace;
            Name = name;
            NestingInfo = new NestingInfo();
            GenericArgs = genericArgs
                .ThrowIfAny(
                    metadata => metadata is null,
                    _ => new InvalidOperationException("Inalid metadata: null"))
                .ToImmutableArray();
        }

        public static InterfaceMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.Interface != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.Interface}'");

            return new InterfaceMetadata(
                type.GetAccessModifier(),
                type.Namespace,
                type.Name,
                type.GetGenericArgumentTypeMetadata());
        }

        public string FullName()
        {
            return new StringBuilder()
                .Append(Namespace)
                .Append(string.IsNullOrWhiteSpace(Namespace) ? "" : ".")
                .Append(SimpleName())
                .ToString();
        }

        public string SimpleName()
        {
            var sbuilder = new StringBuilder(Name);

            if (GenericArgs.Length > 0)
            {
                sbuilder.Append('<');
                for (int cnt = 0; cnt < GenericArgs.Length; cnt++)
                {
                    if (cnt > 0)
                        sbuilder.Append(", ");

                    sbuilder.Append(GenericArgs[cnt].FullName());
                }
                sbuilder.Append('>');
            }

            return sbuilder.ToString();
        }
    }

    public readonly struct TypeParameterMetadata : ITypeMetadata<TypeParameterMetadata>
    {
        public TypeKind Kind => TypeKind.TypeParameter;

        public string Name { get; }

        public TypeParameterMetadata(string name)
        {
            Name = name;
        }

        public static TypeParameterMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.TypeParameter != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.TypeParameter}'");

            return new(type.Name);
        }

        public static TypeParameterMetadata Of(string genericTypeName) => new(genericTypeName);

        public string FullName() => Name;

        public string SimpleName() => Name;
    }

    public readonly struct EnumMetadata : ITypeMetadata<EnumMetadata>
    {
        public TypeKind Kind => TypeKind.Enum;

        public AccessModifier AccessModifier { get; }

        public string? Namespace { get; }

        public string Name { get; }

        public EnumMetadata(
            AccessModifier accessModifier,
            string? @namespace,
            string name)
        {
            AccessModifier = accessModifier;
            Namespace = @namespace;
            Name = name;
        }

        public static EnumMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.Enum != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.Enum}'");

            return new EnumMetadata(
                type.GetAccessModifier(),
                type.Namespace,
                type.Name);
        }

        public string FullName()
        {
            return new StringBuilder()
                .Append(Namespace)
                .Append(string.IsNullOrWhiteSpace(Namespace) ? "" : ".")
                .Append(SimpleName())
                .ToString();
        }

        public string SimpleName() => Name;
    }

    public readonly struct ArrayMetadata : ITypeMetadata<ArrayMetadata>
    {
        public TypeKind Kind => TypeKind.Array;

        public ITypeMetadata ArrayType { get; }

        public int Dimensions { get; }

        public ArrayMetadata(
            ITypeMetadata arrayType,
            int dimensions = 1)
        {
            ArrayType = arrayType;
            Dimensions = dimensions;
        }

        public static ArrayMetadata Of(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var kind = type.GetTypeKind();
            if (TypeKind.Enum != kind)
                throw new InvalidOperationException(
                    $"Invalid type kind: '{kind}', expected '{TypeKind.Enum}'");

            return new ArrayMetadata(
                dimensions: type.GetArrayRank(),
                arrayType: type
                    .GetElementType()!
                    .ToTypeMetadata());
        }

        public string FullName()
        {
            return new StringBuilder()
                .Append(ArrayType.FullName())
                .Append('[')
                .Append("".PadLeft(Dimensions - 1, ','))
                .Append(']')
                .ToString();
        }

        public string SimpleName()
        {
            return new StringBuilder()
                .Append(ArrayType.SimpleName())
                .Append('[')
                .Append("".PadLeft(Dimensions - 1, ','))
                .Append(']')
                .ToString();
        }
    }
}
