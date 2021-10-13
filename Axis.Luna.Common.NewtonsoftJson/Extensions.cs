using Axis.Luna.Common.Types.Basic;

namespace Axis.Luna.Common.NewtonsoftJson
{
    internal static class Extensions
    {
        internal static char Char(this BasicStructJsonConverter.MetadataSymbols symbol) => (char)symbol;

        internal static BasicMetadata ToBasicMetadata(this BasicStructJsonConverter.IMetadata metadata) => new BasicMetadata(metadata.Key, metadata.Value);
    }
}
