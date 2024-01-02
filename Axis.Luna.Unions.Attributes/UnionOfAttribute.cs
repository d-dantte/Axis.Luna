using Axis.Luna.Unions.Attributes.Metadata;

namespace Axis.Luna.Unions.Attributes
{
    /// <summary>
    /// A <c>UnionOF</c> attribute expresses the intention to create a union of the type
    /// given as a parameter to the attribute. The parameter is either a <see cref=""System.Type""/>,
    /// or a <see cref=""System.String""/> instance. Types are used to indicate a concrete type that 
    /// will participate in the union formation, while strings indicate generic type args that
    /// must be present in the target type itself. For generic arguments to be recognized, they need
    /// to have the same value as what appears on the target type.
    /// <para/>
    /// When used, <see cref=""UnionOfAttribute""/> instances must appear 2 or more times for the code
    /// generator to recognize it as a valid use-case, and when it is recognized, the order of the
    /// attributes is the same order that the union types will appear in the relevant <c>*Match</c>
    /// methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
    public class UnionOfAttribute : Attribute
    {
        public ITypeMetadata TypeMetadata { get; }

        public UnionOfAttribute(string genericTypeArg) 
        {
            if (string.IsNullOrWhiteSpace(genericTypeArg))
                throw new ArgumentException($"Invalid type arg name: null/whitespace");

            TypeMetadata = new TypeParameterMetadata(genericTypeArg);
        }

        /// <summary>
        /// TODO: describe what concrete types are. For context, look at <see cref="Metadata.TypeMetadataExtensions.GetTypeKind(Type)"/>
        /// </summary>
        /// <param name="concretTypeArg"></param>
        public UnionOfAttribute(Type concretTypeArg)
        {
            TypeMetadata = concretTypeArg.ToTypeMetadata();
        }
    }
}
