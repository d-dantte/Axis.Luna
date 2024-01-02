using Axis.Luna.Unions.Attributes.Metadata;
using System.Collections.Immutable;

namespace Axis.Luna.Unions
{
    /// <summary>
    /// Holds information needed to build a union type
    /// </summary>
    internal readonly struct UnionMetadata
    {
        /// <summary>
        /// The individual types participating in the union structure
        /// </summary>
        public ImmutableArray<ITypeMetadata> UnionTypes { get; }

        /// <summary>
        /// The type that is being generated as a union
        /// </summary>
        public IProperTypeMetadata TargetType { get; }


        internal UnionMetadata(
            IProperTypeMetadata targetType,
            ImmutableArray<ITypeMetadata> unionTypes)
        {
            TargetType = targetType;
            UnionTypes = unionTypes;

            #region Soft Validation
            // validate types: make sure any generic type in the "typeGenericArgs" list
            // is present as a generic type in the "unionITypeMetadatas" list
            var unionGenericTypes = UnionTypes
                .Where(arg => arg switch
                {
                    IProperTypeMetadata meta => meta.Arity > 0,
                    _ => false
                })
                .ToHashSet();

            // Confirm if throwing exceptions is the proper way to abort operations
            // for analysers
            if (!unionGenericTypes.IsSubsetOf(TargetType.GenericArgs))
                throw new InvalidOperationException(
                    $"Invalid metadata: all generic args must appear among the union attributes");
            #endregion
        }

        #region Nested types
        internal enum TypeForm
        {
            Struct,
            Class
        }
        #endregion
    }
}
