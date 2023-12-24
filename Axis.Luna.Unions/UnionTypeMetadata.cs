using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Axis.Luna.Unions
{
    /// <summary>
    /// Holds information needed to build a union type
    /// </summary>
    internal readonly struct UnionTypeMetadata
    {
        public ImmutableArray<TypeArg> UnionTypeArgs { get; }
        public ImmutableArray<TypeArg> TypeGenericArgs { get; }

        public string TypeNamespace { get; }

        public string TypeName { get; }

        public int TypeArity => TypeGenericArgs.Length;

        internal TypeForm Form { get; }


        internal UnionTypeMetadata(
            string typeNamespace,
            string typeName,
            TypeForm form,
            ImmutableArray<TypeArg> typeGenericArgs,
            ImmutableArray<TypeArg> unionTypeArgs)
        {
            TypeNamespace = typeNamespace;
            TypeName = typeName;
            Form = form;
            TypeGenericArgs = typeGenericArgs;
            UnionTypeArgs = unionTypeArgs;

            // validate types: make sure any generic type in the "typeGenericArgs" list
            // is present as a generic type in the "unionTypeArgs" list
            var unionTypeArgSet = UnionTypeArgs
                .Where(arg => arg.IsGeneric)
                .ToHashSet();

            // Confirm if throwing exceptions is the proper way to abort operations
            // for analysers
            if (!unionTypeArgSet.IsSubsetOf(TypeGenericArgs))
                throw new InvalidOperationException(
                    $"Invalid metadata: all generic args must appear among the union attributes");
        }

        public string GenerateImplementation()
        {
            return TypeTemplate(
                TypeNamespace,
                TypeShape(),
                TypeGenericArgs.Length > 0 ? TypeDeclaration() : TypeName,
                TypeName,
                GenerateUnionMethods(),
                GenerateMapMatchMethod(),
                GenerateConsumeMatchMethod(),
                GenerateWithMatchMethod());
        }

        #region Generators

        private string GenerateUnionMethods()
        {
            var sbuilder = new StringBuilder();

            for (int cnt = 0; cnt < UnionTypeArgs.Length; cnt++)
            {
                var unionType = UnionTypeArgs[cnt];

                //region
                sbuilder
                    .AppendLine()
                    .AppendLine()
                    .Append(Indent(2))
                    .Append("#region ")
                    .Append(unionType);

                // Of
                var template = GenerateOfMethod(unionType);
                sbuilder
                    .Append(Indent(2))
                    .Append(template);

                // implicit
                if (!(unionType.Is(out Type? t) && t!.IsInterface))
                {
                    template = GenerateImplicitOperator(unionType);
                    sbuilder
                        .Append(Indent(2))
                        .Append(template);
                }

                // Is
                template = GenerateIsMethod(unionType);
                sbuilder
                    .Append(Indent(2))
                    .Append(template);

                // end region
                sbuilder
                    .Append(Indent(2))
                    .Append("#endregion");
            }

            return sbuilder.ToString();
        }

        private string GenerateMapMatchMethod()
        {
            var outputGenericType = OutputGenericType();
            var unionMapperDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var mapperInvocations = new StringBuilder();

            for (int cnt = 0; cnt < UnionTypeArgs.Length; cnt++)
            {
                var typeArg = UnionTypeArgs[cnt].TypeDeclaration();
                var argName = $"mapper{cnt}";
                unionMapperDelegateArgs.Append(MapperDelegateTemplate(
                    typeArg,
                    outputGenericType,
                    argName));

                argumentNullInvocations.Append(
                    ArgumentNullExceptionTemplate(argName));

                mapperInvocations.Append(MapperInvocationTemplate(
                    argName,
                    typeArg,
                    cnt));
            }

            return MapMatchTemplate(
                outputGenericType,
                unionMapperDelegateArgs.ToString(),
                argumentNullInvocations.ToString(),
                mapperInvocations.ToString());
        }

        private string GenerateConsumeMatchMethod()
        {
            var unionConsumerDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var consumerInvocations = new StringBuilder();

            for (int cnt = 0; cnt < UnionTypeArgs.Length; cnt++)
            {
                var typeArg = UnionTypeArgs[cnt].TypeDeclaration();
                var argName = $"consumer{cnt}";
                unionConsumerDelegateArgs.Append(ConsumerDelegateTemplate(
                    typeArg,
                    argName));

                argumentNullInvocations.Append(
                    ArgumentNullExceptionTemplate(argName));

                consumerInvocations.Append(ConsumerInvocationTemplate(
                    argName,
                    typeArg,
                    cnt));
            }

            return ConsumeMatchTemplate(
                unionConsumerDelegateArgs.ToString(),
                argumentNullInvocations.ToString(),
                consumerInvocations.ToString());
        }

        private string GenerateWithMatchMethod()
        {
            var unionConsumerDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var consumerInvocations = new StringBuilder();

            for (int cnt = 0; cnt < UnionTypeArgs.Length; cnt++)
            {
                var typeArg = UnionTypeArgs[cnt].TypeDeclaration();
                var argName = $"consumer{cnt}";
                unionConsumerDelegateArgs.Append(ConsumerDelegateTemplate(
                    typeArg,
                    argName));

                argumentNullInvocations.Append(
                    ArgumentNullExceptionTemplate(argName));

                consumerInvocations.Append(ConsumerInvocationTemplate(
                    argName,
                    typeArg,
                    cnt));
            }

            return WithMatchTemplate(
                TypeDeclaration(),
                unionConsumerDelegateArgs.ToString(),
                argumentNullInvocations.ToString(),
                consumerInvocations.ToString());
        }

        private string GenerateOfMethod(
            TypeArg arg)
            => OfTemplate(TypeDeclaration(), arg.TypeDeclaration());

        private string GenerateImplicitOperator(
            TypeArg arg)
            => ImplicitTemplate(TypeDeclaration(), arg.TypeDeclaration());

        private static string GenerateIsMethod(
            TypeArg arg)
            => IsTemplate(arg.TypeDeclaration());
        #endregion

        #region Code Templates

        public static string TypeTemplate(
            string @namespace,
            string shape,
            string typeDeclaration,
            string constructorName,
            string unionTypeMethods,
            string mapMatchMethod,
            string consumeMatchMethod,
            string withMatchMethod)
        {
            return $@"
namespace {@namespace}
{{
    public {shape} {typeDeclaration}
    {{
        /// <summary>
        /// The encapsulated value
        /// </summary>
        public object Value{{ get; }}

        /// <summary>
        /// Construct a new instance of the union type
        /// </summary>
        private {constructorName}(object value)
        {{
            Value = value;
        }}

        public bool IsNull() => Value is null;

        #region Type-Specific methods{unionTypeMethods}

        #endregion{mapMatchMethod}{consumeMatchMethod}{withMatchMethod}
    }}
}}";
        }

        public static string OfTemplate(
            string fullTypeName,
            string unionTypeArgName)
        {
            return $@"

        public static {fullTypeName} Of({unionTypeArgName} value) => new(value);";
        }

        public static string ImplicitTemplate(
            string fullTypeName,
            string unionTypeArgName)
        {
            return $@"

        public static implicit operator {fullTypeName}({unionTypeArgName} value) => new(value);";
        }

        public static string IsTemplate(string unionTypeArgName)
        {
            return $@"

        public bool Is(out {unionTypeArgName} value)
        {{
            if (Value is {unionTypeArgName} unionValue)
            {{
                value = unionValue;
                return true;
            }}

            value = default;
            return false;
        }}";
        }

        public static string MapMatchTemplate(
            string outputGenericType,
            string unionMapperDelegateArgs,
            string argumentNullExceptions,
            string mapperInvocation)
        {
            return $@"

        public {outputGenericType} MapMatch<{outputGenericType}>({unionMapperDelegateArgs}
            Func<{outputGenericType}> defaultMapper)
        {{{argumentNullExceptions}
            ArgumentNullException.ThrowIfNull(defaultMapper);
            {mapperInvocation}

            // unknown type, assume null
            return defaultMapper.Invoke();
        }}";
        }

        public static string ConsumeMatchTemplate(
            string unionMapperDelegateArgs,
            string argumentNullExceptions,
            string consumerInvocation)
        {
            return $@"

        public void ConsumeMatch({unionMapperDelegateArgs}
            Action defaultConsumer = null)
        {{{argumentNullExceptions}
            {consumerInvocation}

            // unknown type, assume null
            else defaultConsumer?.Invoke();
        }}";
        }

        public static string WithMatchTemplate(
            string unionTypeName,
            string unionMapperDelegateArgs,
            string argumentNullExceptions,
            string consumerInvocation)
        {
            return $@"

        public {unionTypeName} WithMatch({unionMapperDelegateArgs}
            Action defaultConsumer = null)
        {{{argumentNullExceptions}
            {consumerInvocation}

            // unknown type, assume null
            else defaultConsumer?.Invoke();

            return this;
        }}";
        }

        public static string MapperDelegateTemplate(
            string unionTypeName,
            string outputGenericType,
            string argName)
        {
            return $@"
            Func<{unionTypeName}, {outputGenericType}> {argName},";
        }

        public static string ConsumerDelegateTemplate(
            string unionTypeName,
            string argName)
        {
            return $@"
            Action<{unionTypeName}> {argName},";
        }

        public static string ArgumentNullExceptionTemplate(
            string argName)
        {
            return $@"
            ArgumentNullException.ThrowIfNull({argName});";
        }

        public static string MapperInvocationTemplate(
            string unionMapper,
            string unionTypeName,
            int unionTypeIndex)
        {
            return $@"

            {(unionTypeIndex > 0 ? "else ": "")}if (Value is {unionTypeName} v{unionTypeIndex})
                return {unionMapper}.Invoke(v{unionTypeIndex});";
        }

        public static string ConsumerInvocationTemplate(
            string unionConsumer,
            string unionTypeName,
            int unionTypeIndex)
        {
            return $@"

            {(unionTypeIndex > 0 ? "else " : "")}if (Value is {unionTypeName} v{unionTypeIndex})
                {unionConsumer}.Invoke(v{unionTypeIndex});";
        }

        #endregion

        #region Helpers

        private static string Indent(int count) => "".PadLeft(count, '\t');

        private string TypeShape() => Form switch
        {
            TypeForm.Struct => "readonly partial struct",
            TypeForm.Class
            or _ => "partial class"
        };

        private string TypeDeclaration()
        {
            var sb = new StringBuilder(TypeName);
            if (TypeGenericArgs.Length > 0)
            {
                sb.Append('<');
                for (int cnt = 0; cnt < TypeGenericArgs.Length; cnt++)
                {
                    if (cnt > 0)
                        sb = sb.Append(", ");
                    sb = sb.Append(TypeGenericArgs[cnt]);
                }
                sb.Append('>');
            }
            return sb.ToString();
        }

        private string OutputGenericType()
        {
            var genericTypes = new HashSet<TypeArg>();
            for (int cnt = 0; cnt < TypeGenericArgs.Length; cnt++)
            {
                if (TypeGenericArgs[cnt].IsGeneric)
                    genericTypes.Add(TypeGenericArgs[cnt]);
            }

            string outputType = null!;
            var index = 0;

            do outputType = $"TOut_{index:x}";
            while (genericTypes.Contains(new TypeArg(outputType)));

            return outputType;
        }
        #endregion

        #region Nested types
        internal enum TypeForm
        {
            Struct,
            Class
        }
        #endregion
    }
}
