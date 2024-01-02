namespace Axis.Luna.Unions.Attributes.Metadata
{
    /// <summary>
    /// 
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        /// Type's kind is undefined.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Type is an array type.
        /// </summary>
        Array,

        /// <summary>
        /// Type is a class.
        /// </summary>
        Class,

        /// <summary>
        /// Type is an enumeration.
        /// </summary>
        Enum,

        /// <summary>
        /// Type is an interface.
        /// </summary>
        Interface,

        /// <summary>
        /// Type is a C# struct or VB Structure
        /// </summary>
        Struct,

        /// <summary>
        /// Type is a generic type parameter
        /// </summary>
        TypeParameter
    }
}
