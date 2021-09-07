using Axis.Luna.Common.Types.Base;
using Axis.Luna.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axis.Luna.Common.NewtonsoftJson
{

    /// <summary>
    /// 
    /// </summary>
    public class StructDataJsonConverter : JsonConverter
    {

        public OverloadedTypeOutputEmbedingStyle OverloadedTypeEmbedingStyle { get; set; }

        public DateTimeParseInfo ParseInfo { get; set; } = new DateTimeParseInfo();

        public override bool CanConvert(Type objectType) => typeof(StructData).Equals(objectType);

        #region Read
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            return ToStructData(jobj);
        }

        private static StructData ToStructData(JObject jobject)
        {
            var jenumerable = jobject as IEnumerable<KeyValuePair<string, JToken>>;

            var explicitPropertyTypes = jenumerable
                .Where(kvp => kvp.Key.StartsWith("@"))
                .Select(kvp => kvp.Key.TrimStart('@').ValuePair(ToDataTypes(kvp.Value.Value<string>())))
                .ToDictionary();

            return jenumerable
                .Where(kvp => !kvp.Key.StartsWith("@"))
                .Select(kvp => ToDataProperty(kvp, explicitPropertyTypes))
                .Aggregate(
                    new StructData(),
                    (data, prop) => data.Append(prop));
        }

        private static KeyValuePair<string, DataType> ToDataProperty(
            KeyValuePair<string, JToken> property,
            Dictionary<string, DataTypes> explicitPropertyTypes)
        {
            var type =
                explicitPropertyTypes.ContainsKey(property.Key) ? explicitPropertyTypes[property.Key] :
                property.Key.Contains(":") ? ToDataTypes(property.Key.Split(':')[0]) :
                ToDataTypes(property.Value.Type);

            var propertyName = property.Key.Contains(":")
                ? property.Key.Split(':')[1]
                : property.Key;

            return new KeyValuePair<string, DataType>(
                propertyName,
                ToDataValue(type, property.Value));
        }

        private static DataType ToDataValue(DataTypes type, JToken token)
        {
            return type switch
            {
                DataTypes.Bool => token.Value<bool>(),
                DataTypes.Bytes => token.Value<byte[]>(),
                DataTypes.Date => DateTimeOffset.Parse(token.Value<string>()),
                DataTypes.Decimal => token.Value<decimal>(),
                DataTypes.Guid => Guid.Parse(token.Value<string>()),
                DataTypes.Int => token.Value<int>(),
                DataTypes.Real => token.Value<double>(),
                DataTypes.String => token.Value<string>(),
                DataTypes.TimeSpan => TimeSpan.Parse(token.Value<string>()),

                DataTypes.List => token
                    .As<JArray>()
                    .Select(item => ToDataValue(ToDataTypes(item.Type), item))
                    .ToArray(),

                DataTypes.Struct => ToStructData(token as JObject),

                _ => new StringData { Value = null }
            };
        }
        #endregion

        #region Write
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var @struct = value as StructData;
            var jobject = ToJObject(@struct);

            jobject.WriteTo(writer);
        }

        private JToken ToJToken(DataType dataType)
        {
            if (dataType == null)
                return JValue.CreateNull();

            //else
            return dataType switch
            {
                BoolData @bool => new JValue(@bool.Value),

                IntData @int => new JValue(@int.Value),

                DecimalData @decimal => new JValue(@decimal.Value),

                RealData real => new JValue(real.Value),

                StringData @string => new JValue(@string.Value),

                GuidData guid => new JValue(guid.Value),

                DateData date => new JValue(date.Value?.ToString(ParseInfo.Formats[0])),

                TimeSpanData timespan => new JValue(timespan.Value),

                ListData list => list.Value
                    .Select(ToJToken)
                    .ToArray()
                    .Map(jarr => new JArray(jarr)),

                StructData @struct => @struct.Value
                    .SelectMany(ToJProperties)
                    .Aggregate(new JObject(), (jobj, prop) =>
                    {
                        jobj[prop.Key] = prop.Value;
                        return jobj;
                    }),

                _ => throw new ArgumentException($"Invalid dataType: {dataType?.Type}"),
            };
        }

        private KeyValuePair<string, JToken>[] ToJProperties(KeyValuePair<string, DataType> property)
        {
            var propertyList = new List<KeyValuePair<string, JToken>>();

            switch (property.Value.Type)
            {
                case DataTypes.Decimal:
                case DataTypes.Bytes:
                case DataTypes.Guid:
                case DataTypes.Date:
                case DataTypes.TimeSpan:
                    if (OverloadedTypeEmbedingStyle == OverloadedTypeOutputEmbedingStyle.Explicit)
                        propertyList.Add($"@{property.Key}".ValuePair(new JValue(property.Value.Type.ToString()) as JToken));

                    var prefix = OverloadedTypeEmbedingStyle == OverloadedTypeOutputEmbedingStyle.PropertyName
                        ? $"{property.Value.Type}:"
                        : "";

                    propertyList.Add($"{prefix}{property.Key}".ValuePair(ToJToken(property.Value)));
                    break;


                default:
                    propertyList.Add(property.Key.ValuePair(ToJToken(property.Value)));
                    break;
            }

            return propertyList.ToArray();
        }

        private JObject ToJObject(StructData @struct)
        {
            var explicitPropertyTypes = @struct.Value
                .Where(prop =>
                {
                    return prop.Value.Type switch
                    {
                        DataTypes.Bytes => true,
                        DataTypes.Date => true,
                        DataTypes.Decimal => true,
                        DataTypes.Real => true,
                        DataTypes.Guid => true,
                        DataTypes.Int => false,
                        DataTypes.List => false,
                        DataTypes.String => true,
                        DataTypes.Struct => false,
                        DataTypes.TimeSpan => true,
                        _ => false
                    };
                })
                .Select(kvp => kvp.Key.ValuePair(kvp.Value.Type))
                .ToDictionary();

            return @struct.Value
                .SelectMany(ToJProperties)
                .Aggregate(new JObject(), (jobj, prop) =>
                {
                    jobj[prop.Key] = prop.Value;
                    return jobj;
                });
        }

        public static JObject ToJObject(StructData @struct, OverloadedTypeOutputEmbedingStyle embedingStyle)
        {
            var converter = new StructDataJsonConverter { OverloadedTypeEmbedingStyle = embedingStyle };
            return converter.ToJObject(@struct);
        }
        #endregion

        private static DataTypes ToDataTypes(string typeName)
        {
            if (Enum.TryParse<DataTypes>(typeName, out var dataType))
                return dataType;

            else
                throw new ArgumentException($"Invalid {nameof(typeName)}: {typeName}");
        }

        private static DataTypes ToDataTypes(JTokenType tokenType)
        {
            return tokenType switch
            {
                JTokenType.Object => DataTypes.Struct,
                JTokenType.Array => DataTypes.List,
                JTokenType.Integer => DataTypes.Int,
                JTokenType.Float => DataTypes.Real,
                JTokenType.String => DataTypes.String,
                JTokenType.Boolean => DataTypes.Bool,
                JTokenType.Date => DataTypes.Date,
                JTokenType.Bytes => DataTypes.Bytes,
                JTokenType.Guid => DataTypes.Guid,
                JTokenType.TimeSpan => DataTypes.TimeSpan,
                _ => DataTypes.String,
            };
        }


        #region Nested Types

        /// <summary>
        /// Used when writing properties into the json objects.
        /// Reading accomodates both styles - first reading from the explicit properties, then from the property names.
        /// </summary>
        public enum OverloadedTypeOutputEmbedingStyle
        {
            /// <summary>
            /// Do not embed the type
            /// </summary>
            None,

            /// <summary>
            /// Embed the type in the property name, i.e, {"Decimal:debitAmount": 654.43}
            /// </summary>
            PropertyName,

            /// <summary>
            /// Embed the type in a separate property in the same object, i.e, {"debitAmount": 654.43, "@debitAmount": "Decimal"}
            /// </summary>
            Explicit
        }

        public class DateTimeParseInfo
        {
            /// <summary>
            /// All formats to try when parsing. Use the first when serializing. This property cannot be null or empty
            /// </summary>
            public string[] Formats { get; }

            /// <summary>
            /// Culture info to use while parsing datetime values
            /// </summary>
            public CultureInfo CultureInfo { get; }

            /// <summary>
            /// DateTimeStyles for parsing datatime values
            /// </summary>
            public DateTimeStyles Styles { get; }


            public DateTimeParseInfo(
                CultureInfo info,
                DateTimeStyles style,
                params string[] formats)
            {
                CultureInfo = info ?? throw new ArgumentNullException(nameof(info));
                Styles = style;
                Formats = formats
                    ?.ToArray()
                    .ThrowIf(
                        arr => arr == null || arr.Length == 0,
                        new ArgumentException($"Invalid format array: [{Formats?.Length}]"));
            }

            public DateTimeParseInfo(
                DateTimeStyles style,
                params string[] formats)
                : this(CultureInfo.InvariantCulture, style, formats)
            { }

            public DateTimeParseInfo(params string[] formats)
                : this(CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, formats)
            { }

            public DateTimeParseInfo()
                : this(
                      CultureInfo.InvariantCulture,
                      DateTimeStyles.AssumeUniversal,
                      "yyyy-MM-dd HH:mm:ss.fffffff zzz",
                      "yyyy-MM-dd HH:mm:ss.fffffff",
                      "yyyy-MM-dd HH:mm:ss",
                      "yyyy-MM-dd HH:mm",
                      "yyyy-MM-dd")
            { }
        }
        #endregion
    }
}
