using Axis.Luna.Unions.Attributes.Metadata;
using System.Collections.Immutable;
using System.Text;

namespace Axis.Luna.Unions.SourceGenerator
{
    internal static class UnionMetadataSourceGenerator
    {

        public static string GenerateImplementation(UnionMetadata unionMetadata)
        {
            return TypeTemplate(
                unionMetadata.TargetType.Namespace,
                TypeShape(unionMetadata.TargetType),
                unionMetadata.TargetType.SimpleName(),
                unionMetadata.TargetType.Name,
                GenerateUnionAPIMethods(unionMetadata),
                GenerateMapMatchMethod(unionMetadata.UnionTypes),
                GenerateConsumeMatchMethod(unionMetadata.UnionTypes),
                GenerateWithMatchMethod(unionMetadata));
        }

        #region Generators

        private static string GenerateUnionAPIMethods(UnionMetadata unionMetadata)
        {
            var sbuilder = new StringBuilder();

            for (int cnt = 0; cnt < unionMetadata.UnionTypes.Length; cnt++)
            {
                var unionType = unionMetadata.UnionTypes[cnt];

                //region
                sbuilder
                    .AppendLine()
                    .AppendLine()
                    .Append(Indent(2))
                    .Append("#region ")
                    .Append(unionType.FullName());

                // Of
                var template = GenerateOfMethod(unionMetadata.TargetType, unionType);
                sbuilder
                    .Append(Indent(2))
                    .Append(template);

                // implicit
                if (unionType is not InterfaceMetadata)
                {
                    template = GenerateImplicitOperator(unionMetadata.TargetType, unionType);
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

        private static string GenerateMapMatchMethod(ImmutableArray<ITypeMetadata> unionTypes)
        {
            var outputGenericType = OutputGenericType(unionTypes);
            var unionMapperDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var mapperInvocations = new StringBuilder();

            for (int cnt = 0; cnt < unionTypes.Length; cnt++)
            {
                var typeArg = unionTypes[cnt].FullName();
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

        private static string GenerateConsumeMatchMethod(ImmutableArray<ITypeMetadata> unionTypes)
        {
            var unionConsumerDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var consumerInvocations = new StringBuilder();

            for (int cnt = 0; cnt < unionTypes.Length; cnt++)
            {
                var typeArg = unionTypes[cnt].FullName();
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

        private static string GenerateWithMatchMethod(UnionMetadata unionMetadata)
        {
            var unionConsumerDelegateArgs = new StringBuilder();
            var argumentNullInvocations = new StringBuilder();
            var consumerInvocations = new StringBuilder();

            for (int cnt = 0; cnt < unionMetadata.UnionTypes.Length; cnt++)
            {
                var typeArg = unionMetadata.UnionTypes[cnt].FullName();
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
                unionMetadata.TargetType.FullName(),
                unionConsumerDelegateArgs.ToString(),
                argumentNullInvocations.ToString(),
                consumerInvocations.ToString());
        }

        private static string GenerateOfMethod(
            IProperTypeMetadata targetType,
            ITypeMetadata unionType)
            => OfTemplate(targetType.FullName(), unionType.FullName());

        private static string GenerateImplicitOperator(
            IProperTypeMetadata targetType,
            ITypeMetadata unionType)
            => ImplicitTemplate(targetType.FullName(), unionType.FullName());

        private static string GenerateIsMethod(
            ITypeMetadata unionType)
            => IsTemplate(unionType.FullName());
        #endregion

        #region Code Templates

        public static string TypeTemplate(
            string? @namespace,
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
            string unionITypeMetadataName)
        {
            return $@"

        public static {fullTypeName} Of({unionITypeMetadataName} value) => new(value);";
        }

        public static string ImplicitTemplate(
            string fullTypeName,
            string unionITypeMetadataName)
        {
            return $@"

        public static implicit operator {fullTypeName}({unionITypeMetadataName} value) => new(value);";
        }

        public static string IsTemplate(string unionITypeMetadataName)
        {
            return $@"

        public bool Is(out {unionITypeMetadataName} value)
        {{
            if (Value is {unionITypeMetadataName} unionValue)
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

            {(unionTypeIndex > 0 ? "else " : "")}if (Value is {unionTypeName} v{unionTypeIndex})
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

        private static string TypeShape(IProperTypeMetadata metadata) => metadata switch
        {
            StructMetadata => "readonly partial struct",
            ClassMetadata => "partial class",
            _ => throw new InvalidOperationException(
                $"Invalid target type: {metadata?.GetType()}")
        };

        private static string OutputGenericType(ImmutableArray<ITypeMetadata> genericArgs)
        {
            var genericTypes = new HashSet<ITypeMetadata>();
            for (int cnt = 0; cnt < genericArgs.Length; cnt++)
            {
                if (genericArgs[cnt] is TypeParameterMetadata)
                    genericTypes.Add(genericArgs[cnt]);
            }

            var index = 0;
            string outputType;

            do outputType = $"TOut_{index:x}";
            while (genericTypes.Contains(new TypeParameterMetadata(outputType)));

            return outputType;
        }
        #endregion
    }
}
