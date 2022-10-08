using Axis.Luna.Common.NewtonsoftJson;
using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Axis.Luna.Operation.NewtonsoftJson
{
    public class OperationErrorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(OperationError).Equals(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jerror = JObject.Load(reader);

            var message = jerror.TryGetValue(nameof(OperationError.Message), out var token)
                ? token.Value<string>()
                : null;

            var code = jerror.TryGetValue(nameof(OperationError.Code), out token)
                ? token.Value<string>()
                : null;

            var data = jerror.TryGetValue(nameof(OperationError.Data), out token)
                ? token.ToObject<BasicStruct>(serializer)
                : (BasicStruct?)null;

            return new OperationError(
                message,
                code,
                data);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is OperationError error))
                JValue.CreateNull().WriteTo(writer);

            else
            {
                var jobj = ToJToken(error, serializer) as JObject;
                jobj.WriteTo(writer);
            }
        }

        public static JToken ToJToken(OperationError error, JsonSerializer serializer)
        {
            if (error == null)
                return JValue.CreateNull();

            var joperationError = new JObject();

            if (error.Message != null)
                joperationError[nameof(OperationError.Message)] = error.Message;

            if (error.Code != null)
                joperationError[nameof(OperationError.Code)] = error.Code;

            if (error.Data != null)
                joperationError[nameof(OperationError.Data)] = GetBasicStructConverter(serializer).ToJObject(error.Data.Value);

            return joperationError;
        }

        private static BasicStructJsonConverter GetBasicStructConverter(JsonSerializer serializer)
        {
            return serializer.Converters
                .FirstOrDefault(c => c is BasicStructJsonConverter)
                .As<BasicStructJsonConverter>()
                ?? new BasicStructJsonConverter();
        }
    }
}
