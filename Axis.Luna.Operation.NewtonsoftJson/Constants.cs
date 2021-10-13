using Axis.Luna.Common.NewtonsoftJson;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Axis.Luna.Operation.NewtonsoftJson
{
    public static class Constants
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,

            Converters = new List<JsonConverter>
            {
                new OperationJsonConverter(),
                new OperationErrorJsonConverter(),
                new BasicStructJsonConverter
                {
                    ParseInfo = new BasicStructJsonConverter.DateTimeParseInfo()
                }
            }
        };
    }
}
