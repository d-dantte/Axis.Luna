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
        private static readonly string UnionAttributeFullName =
            $"{UnionOfAttributeGenerator.Namespace}"
            + $".{UnionOfAttributeGenerator.AttributeName}";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // First, generate the UnionOf attribute that will be used to decorate classes
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "UnionOfAttribute.g.cs",
                SourceText.From(UnionOfAttributeGenerator.GenerateAttributeClass(), Encoding.UTF8)));

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
                string result = metadata.GenerateImplementation();
                context.AddSource(
                    $"{metadata.TypeName}_{metadata.TypeArity}_Union.g.cs",
                    SourceText.From(result, Encoding.UTF8));
            }
        }

        private static List<UnionTypeMetadata> CreateUnionMetadata(
            Compilation compilation,
            IEnumerable<TypeDeclarationSyntax> typeDeclarations,
            CancellationToken token)
        {
            var metadataList = new List<UnionTypeMetadata>();

            // extract type args
            foreach (var typeDeclaration in typeDeclarations)
            {
                var typeArgs = ExtractUnionTypeArg(
                    compilation,
                    typeDeclaration,
                    token);
            }


        }

        private static List<TypeArg> ExtractUnionTypeArg(
            Compilation compilation,
            TypeDeclarationSyntax typeSyntax,
            CancellationToken token)
        {
            var argList = new List<TypeArg>();
            var semmodel = compilation.GetSemanticModel(typeSyntax.SyntaxTree);

            foreach (var attributeListSyntax in typeSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    var aas = attributeSyntax.ArgumentList!.Arguments[0];
                    argList.Add((aas.Expression.Kind(), aas.Expression) switch
                    {
                        (SyntaxKind.StringLiteralExpression, _) => aas.Expression.ToString(),
                        SyntaxKind.TypeOfExpression => semmodel
                            .GetTypeInfo(
                                ((TypeOfExpressionSyntax)aas.Expression).Type)
                            .Type
                            .
                    });
                }
            }

        }
    }
}
