namespace Axis.Luna.Unions
{
    #region Obsolete. DELETE
    /// <summary>
    /// A <c>UnionOF</c> attribute expresses the intention to create a union of the type
    /// given as a parameter to the attribute. The parameter is either a <see cref="System.Type"/>,
    /// or a <see cref="System.String"/> instance. Types are used to indicate a concrete type that 
    /// will participate in the union formation, while strings indicate generic type args that
    /// must be present in the target type itself. For generic arguments to be recognized, they need
    /// to have the same value as what appears on the target type.
    /// <para/>
    /// When used, <see cref="UnionOfAttribute"/> instances must appear 2 or more times for the code
    /// generator to recognize it as a valid use-case, and when it is recognized, the order of the
    /// attributes is the same order that the union types will appear in the relevant <c>*Match</c>
    /// methods.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Struct | AttributeTargets.Class,
        AllowMultiple = true)]
    public class UnionOfAttributex : Attribute
    {
        internal TypeArg TypeArg { get; }

        public UnionOfAttributex(Type type)
        {
            TypeArg = type;
        }

        public UnionOfAttributex(string genericType)
        {
            TypeArg = genericType;
        }
    }
    #endregion

    public static class UnionOfAttributeGenerator
    {
        internal static readonly string Namespace = "UnionUtils";

        internal static readonly string AttributeName = "UnionOfAttribute";

        public static string GenerateAttributeClass()
        {
            return @$"
namespace {Namespace}
{{
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
    public class {AttributeName} : Attribute
    {{
        public UnionOfAttribute(Type type)
        {{ }}

        public UnionOfAttribute(string genericType)
        {{ }}
    }}  
}}
";
        }
    }
}
