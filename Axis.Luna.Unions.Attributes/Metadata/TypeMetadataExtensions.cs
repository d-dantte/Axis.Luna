namespace Axis.Luna.Unions.Attributes.Metadata
{
    public static class TypeMetadataExtensions
    {
        public static ITypeMetadata ToTypeMetadata(this Type type)
        {
            return type?.GetTypeKind() switch
            {
                TypeKind.Array => ArrayMetadata.Of(type),
                TypeKind.Class => ClassMetadata.Of(type),
                TypeKind.Enum => EnumMetadata.Of(type),
                TypeKind.Interface => InterfaceMetadata.Of(type),
                TypeKind.Struct => StructMetadata.Of(type),
                TypeKind.TypeParameter => TypeParameterMetadata.Of(type),
                _ => null!
            };
        }

        public static ITypeMetadata[] GetGenericArgumentTypeMetadata(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            return type
                .GetGenericArguments()
                .Select(ToTypeMetadata)
                .ToArray();
        }

        internal static AccessModifier GetAccessModifier(this Type type)
        {
            if (type.IsNestedPrivate)
                return AccessModifier.Private;

            if (type.IsNestedFamily)
                return AccessModifier.Protected;

            if (type.IsNestedFamORAssem)
                return AccessModifier.ProtectedInternal;

            if (type.IsNestedFamANDAssem)
                return AccessModifier.PrivateProtected;

            if ((type.IsNotPublic && !type.IsNested) || type.IsNestedAssembly)
                return AccessModifier.Internal;

            if (type.IsPublic || type.IsNestedPublic)
                return AccessModifier.Public;

            return AccessModifier.Unknown;
        }

        internal static TypeKind GetTypeKind(this Type type)
        {
            if (type.IsGenericTypeDefinition)
                return TypeKind.Unknown;

            if (type.IsGenericTypeParameter)
                return TypeKind.TypeParameter;

            if (type.IsArray || type.IsSZArray)
                return TypeKind.Array;

            if (type.IsEnum)
                return TypeKind.Enum;

            if (type.IsInterface)
                return TypeKind.Interface;

            if (type.IsClass)
                return TypeKind.Class;

            if (type.IsValueType)
                return TypeKind.Struct;

            return TypeKind.Unknown;
        }
    }
}
