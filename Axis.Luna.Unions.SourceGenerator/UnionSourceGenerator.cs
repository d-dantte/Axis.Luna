using Axis.Luna.Unions.Attributes;
using Axis.Luna.Unions.Attributes.Metadata;
using Axis.Luna.Unions.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Axis.Luna.Unions
{
    [Generator(LanguageNames.CSharp)]
    public class UnionSourceGenerator : IIncrementalGenerator
    {
        private static readonly string UnionAttributeFullName = typeof(UnionOfAttribute).FullName!;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Create the pipeline for generating Unions
            var unionTypeDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: IsRefOrValueTypeWithAttribute,
                    transform: SelectUnionCandidateTypes) 
                .Where(static m => m is not null)
                .Select((tds, ct) => tds!);

            // Combine the selected types with the `Compilation`
            var compilationAndTypes = context.CompilationProvider.Combine(unionTypeDeclarations.Collect());

            // Generate the source using the compilation and types
            context.RegisterSourceOutput(
                compilationAndTypes,
                static (spc, source) => Execute(source.Left, source.Right, spc));
        }

        /// <summary>
        /// Filter Class or Struct declaration with at least one attribute, and only one base type
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsRefOrValueTypeWithAttribute(
            SyntaxNode node,
            CancellationToken cancellationToken)
            => node switch
            {
                ClassDeclarationSyntax cds => cds.AttributeLists.Count > 0,
                StructDeclarationSyntax sds => sds.AttributeLists.Count > 0,
                _ => false
            };


        /// <summary>
        /// Select only types meeting the following criteria:
        /// <list type="number">
        /// <item>Is decorated with the <see cref="UnionOfAttribute"/>.</item>
        /// <item>Implements any of the "<c>Axis.Common.Unions.IUnion&lt;...&gt;</c>" interfaces.</item>
        /// <item>If "<c>Axis.Common.Unions.IUnion</c>" &amp; "<c>Axis.Common.Unions.IUnionOf</c>" are both implemented, the generic types must match.</item>
        /// </list>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static TypeDeclarationSyntax? SelectUnionCandidateTypes(
            GeneratorSyntaxContext context,
            CancellationToken token)
        {
            var typeSyntax = (TypeDeclarationSyntax)context.Node;

            return
                HasUnionAttributes(typeSyntax, context, token)
                && IsPartial(typeSyntax)
                && IsNotAbstract(typeSyntax)
                ? typeSyntax : null;
        }

        /// <summary>
        /// Checks that the type (class/struct) has at least 2 <see cref="UnionOfAttribute"/> instances decorating it.
        /// </summary>
        /// <param name="typeSyntax"></param>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool HasUnionAttributes(
            TypeDeclarationSyntax typeSyntax,
            GeneratorSyntaxContext context,
            CancellationToken token)
        {
            var attributeCount = 0;
            // loop through all the attributes on the type. Double loop because attributes can appear as:
            // [Att1, Att2, Att3][Att4][Att5, Att5] 
            foreach (var attributeListSyntax in typeSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax, token);

                    // is the symbol present?
                    if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                        continue;

                    var fullName = attributeSymbol.ContainingType.ToDisplayString();

                    if (UnionAttributeFullName.Equals(fullName))
                    {
                        attributeCount++;

                        if (attributeCount >= 2)
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks that this type is declared as partial
        /// </summary>
        /// <param name="typeSyntax"></param>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsPartial(TypeDeclarationSyntax typeSyntax)
        {
            foreach (var modifier in typeSyntax.Modifiers)
            {
                if ("partial".Equals(modifier.ToString()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks that this type is not abstract
        /// </summary>
        /// <param name="typeSyntax"></param>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsNotAbstract(TypeDeclarationSyntax typeSyntax)
        {
            foreach (var modifier in typeSyntax.Modifiers)
            {
                if ("abstract".Equals(modifier.ToString()))
                    return false;
            }

            return true;
        }

        private static void Execute(
            Compilation compilation,
            ImmutableArray<TypeDeclarationSyntax> types,
            SourceProductionContext context)
        {
            if (types.IsDefaultOrEmpty)
                return;

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it,
            // so seems like a good idea!
            var distinctTypes = types.Distinct();

            // Convert each EnumDeclarationSyntax to an EnumToGenerate
            var unionMetadataList = CreateUnionMetadata(
                compilation,
                distinctTypes,
                context.CancellationToken);

            foreach (var metadata in unionMetadataList)
            {
                string result = UnionMetadataSourceGenerator.GenerateImplementation(metadata);
                context.AddSource(
                    $"{metadata.TargetType.Name}_{metadata.TargetType.Arity}_Union.g.cs",
                    SourceText.From(result, Encoding.UTF8));
            }
        }

        private static List<UnionMetadata> CreateUnionMetadata(
            Compilation compilation,
            IEnumerable<TypeDeclarationSyntax> typeDeclarations,
            CancellationToken token)
        {
            var metadataList = new List<UnionMetadata>();

            foreach (var typeSyntax in typeDeclarations)
            {
                var semmodel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);
                var typeSymbol = semmodel.GetDeclaredSymbol(typeSyntax, token)!;

                // extract type args
                var typeArgs = ExtractUnionITypeMetadata(
                    compilation,
                    typeSymbol,
                    token);

                if (!typeSymbol.TryConvertToTypeMetadata(out var tmeta)
                    || tmeta is not IProperTypeMetadata targetMetadata)
                {
                    // report error or abandon process, etc
                    continue;
                }

                // create union metadata
                var unionMetadata = new UnionMetadata(targetMetadata, typeArgs.ToImmutableArray());
                metadataList.Add(unionMetadata);
            }

            return metadataList;
        }

        private static List<ITypeMetadata> ExtractUnionITypeMetadata(
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            CancellationToken token)
        {
            var argList = new List<ITypeMetadata>();

            // Extract the type symbol
            var typeAttributes = typeSymbol.GetAttributes();
            var unionAttribute = compilation.GetTypeByMetadataName(UnionAttributeFullName)!;

            for (int cnt = 0; cnt < typeAttributes.Length; cnt++)
            {
                // do something with the CancellationToken

                var attributeData = typeAttributes[cnt];

                if (!unionAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                    continue;

                // check the constructor arg
                var typedConstant = attributeData.ConstructorArguments[0];

                // generic
                if (TypedConstantKind.Primitive.Equals(typedConstant.Kind))
                    argList.Add(TypeParameterMetadata.Of(typedConstant.Value!.ToString()!)); // expect a string

                // concrete
                if (TypedConstantKind.Type.Equals(typedConstant.Kind)
                    && typedConstant.Value is INamedTypeSymbol argTypeSymbol)
                {
                    if (!argTypeSymbol.TryConvertToTypeMetadata(out var metadata))
                    {
                        // report some sort of error, or abort the entire process.
                    }
                    else argList.Add(metadata);
                }
            }

            return argList;
        }
    }
}
