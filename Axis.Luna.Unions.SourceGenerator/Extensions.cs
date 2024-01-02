using Axis.Luna.Unions.Attributes.Metadata;
using Microsoft.CodeAnalysis;
using System.Text;

namespace Axis.Luna.Unions.SourceGenerator
{
    using CSTypeKind = Microsoft.CodeAnalysis.TypeKind;

    internal static class Extensions
    {
        /// <summary>
        /// Create an <see cref="ITypeMetadata"/> instance from the <see cref="INamedTypeSymbol"/>. 
        /// <para/>
        /// NOTE: This method also validates the infomration passed into the individual metadata instances.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        internal static bool TryConvertToTypeMetadata(this
            INamedTypeSymbol typeSymbol,
            out ITypeMetadata metadata)
            => typeSymbol.TryConvertToTypeMetadata(true, out metadata);

        /// <summary>
        /// Create an <see cref="ITypeMetadata"/> instance from the <see cref="INamedTypeSymbol"/>. 
        /// <para/>
        /// NOTE: This method also validates the infomration passed into the individual metadata instances.
        /// </summary>
        /// <param name="includeNesting"></param>
        /// <param name="typeSymbol"></param>
        /// <returns></returns>
        internal static bool TryConvertToTypeMetadata(this
            ITypeSymbol typeSymbol,
            bool includeNestingInfo,
            out ITypeMetadata metadata)
        {
            if (typeSymbol is ITypeParameterSymbol tps
                && tps.TryConvertTypeParameterMetadata(out var tmeta))
            {
                metadata = tmeta;
                return true;
            }
            else if(typeSymbol is IArrayTypeSymbol ats
                && ats.TryConvertArrayMetadata(out var ameta))
            {
                metadata = ameta;
                return true;
            }
            else if (typeSymbol is INamedTypeSymbol nts)
            {
                if (nts.TryConvertEnumsMetadata(out var emeta))
                {
                    metadata = emeta;
                    return true;
                }
                else if (nts.TryConvertStructMetadata(out var smeta))
                {
                    metadata = smeta;
                    return true;
                }
                else if(nts.TryConvertClassMetadata(out var cmeta))
                {
                    metadata = cmeta;
                    return true;
                }
                else if(nts.TryConvertInterfaceMetadata(out var imeta))
                {
                    metadata = imeta;
                    return true;
                }
            }

            metadata = default!;
            return false;
        }

        private static bool TryConvertClassMetadata(this
            INamedTypeSymbol typeSymbol,
            out ClassMetadata metadata)
        {
            if(typeSymbol is not null
                && (CSTypeKind.Class.Equals(typeSymbol.TypeKind)
                || CSTypeKind.Delegate.Equals(typeSymbol.TypeKind)))
                //|| CSTypeKind.FunctionPointer.Equals(typeSymbol.TypeKind))) ??
            {
                metadata = new(
                    typeSymbol.GetAccessModifier(),
                    typeSymbol.GetFullNamespace(),
                    typeSymbol.Name,
                    typeSymbol.TypeParameters
                        .Select(ToTypeParameterMetadata)
                        .Select(metadata => (ITypeMetadata)metadata)
                        .ToArray());
                return true;
            }

            metadata = default;
            return false;
        }

        private static bool TryConvertStructMetadata(this
            INamedTypeSymbol typeSymbol,
            out ClassMetadata metadata)
        {
            if (typeSymbol is not null
                && CSTypeKind.Struct.Equals(typeSymbol.TypeKind))
            {
                metadata = new(
                    typeSymbol.GetAccessModifier(),
                    typeSymbol.GetFullNamespace(),
                    typeSymbol.Name,
                    typeSymbol.TypeParameters
                        .Select(ToTypeParameterMetadata)
                        .Select(metadata => (ITypeMetadata)metadata)
                        .ToArray());
                return true;
            }

            metadata = default;
            return false;
        }

        private static bool TryConvertInterfaceMetadata(this
            INamedTypeSymbol typeSymbol,
            out ClassMetadata metadata)
        {
            if (typeSymbol is not null
                && CSTypeKind.Struct.Equals(typeSymbol.TypeKind))
            {
                metadata = new(
                    typeSymbol.GetAccessModifier(),
                    typeSymbol.GetFullNamespace(),
                    typeSymbol.Name,
                    typeSymbol.TypeParameters
                        .Select(ToTypeParameterMetadata)
                        .Select(metadata => (ITypeMetadata)metadata)
                        .ToArray());
                return true;
            }

            metadata = default;
            return false;
        }

        private static bool TryConvertEnumsMetadata(this
            INamedTypeSymbol typeSymbol,
            out EnumMetadata metadata)
        {
            if (typeSymbol is not null
                && CSTypeKind.Enum.Equals(typeSymbol.TypeKind))
            {
                metadata = new(
                    typeSymbol.GetAccessModifier(),
                    typeSymbol.GetFullNamespace(),
                    typeSymbol.Name);
                return true;
            }

            metadata = default;
            return false;
        }

        private static bool TryConvertTypeParameterMetadata(this
            ITypeParameterSymbol typeSymbol,
            out TypeParameterMetadata metadata)
        {
            if(typeSymbol is not null)
            {
                metadata = new(typeSymbol.Name);
                return true;
            }

            metadata = default;
            return false;
        }


        private static bool TryConvertArrayMetadata(this
            IArrayTypeSymbol typeSymbol,
            out ArrayMetadata metadata)
        {
            if (typeSymbol is not null)
            {
                metadata = new(
                    ((INamedTypeSymbol)typeSymbol.ElementType).ToTypeMetadata(),
                    typeSymbol.Rank);
                return true;
            }

            metadata = default;
            return false;
        }

        #region Helpers

        private static ITypeMetadata ToTypeMetadata(this INamedTypeSymbol symbol)
        {
            if (symbol.TryConvertToTypeMetadata(false, out var metadata))
                return metadata;

            throw new InvalidOperationException($"Invalid symbol: could not convert");
        }

        private static TypeParameterMetadata ToTypeParameterMetadata(this ITypeParameterSymbol symbol)
        {
            if (symbol.TryConvertTypeParameterMetadata(out var metadata))
                return metadata;

            throw new InvalidOperationException("Invalid symbol: could not convert");
        }

        private static AccessModifier GetAccessModifier(this INamedTypeSymbol symbol)
        {
            ArgumentNullException.ThrowIfNull(symbol);
            return symbol.DeclaredAccessibility switch
            {
                Accessibility.Public => AccessModifier.Public,
                Accessibility.Private => AccessModifier.Private,
                Accessibility.Internal => AccessModifier.Internal,
                Accessibility.Protected => AccessModifier.Protected,
                Accessibility.ProtectedOrInternal => AccessModifier.ProtectedInternal,
                Accessibility.ProtectedAndInternal => AccessModifier.PrivateProtected,
                _ or Accessibility.NotApplicable => AccessModifier.Unknown
            };
        }

        private static string GetFullNamespace(this
            INamedTypeSymbol symbol)
            => symbol.ContainingNamespace.GetNamespace();

        private static string GetNamespace(this INamespaceSymbol namespaceSymbol)
        {
            if (namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace)
                return string.Empty;

            var containingNamespace = namespaceSymbol.ContainingNamespace.GetNamespace();
            return new StringBuilder()
                .Append(containingNamespace)
                .Append(string.IsNullOrEmpty(containingNamespace) ? "" : ".")
                .Append(namespaceSymbol.Name)
                .ToString();
        }

        #endregion
    }
}
