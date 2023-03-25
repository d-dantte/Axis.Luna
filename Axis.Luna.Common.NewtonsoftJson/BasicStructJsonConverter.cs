using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using static Axis.Luna.Extensions.ExceptionExtension;

namespace Axis.Luna.Common.NewtonsoftJson
{
    /// <summary>
    /// Serializes the Struct using json format.
    /// Metadata information for the entire object graph is serialized and stored as an extra property of the root <seealso cref="BasicStruct"/>
    /// </summary>
    public class BasicStructJsonConverter : JsonConverter
    {
        public static readonly string MetadataPropertyKey = "@@@";
        public static readonly char MapAccessor = '.';
        public static readonly char ArrayAccessor = '+';

        public DateTimeParseInfo ParseInfo { get; set; } = new DateTimeParseInfo();

        public override bool CanConvert(Type objectType) => typeof(BasicStruct).Equals(objectType);

        #region Read
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            var metadata = ExtractMetadata(
                jobj.TryGetValue(MetadataPropertyKey, out var metadataMap)
                ? metadataMap as JObject
                : null);

            return (BasicStruct)ToBasicValue(jobj, $"{MapAccessor}", metadata);
        }

        private BasicStruct ToBasicStruct(JObject jobject, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            var (_, valueMetadata, _) = ExtractMetadata(path, metadata);

            return jobject
                .As<IDictionary<string, JToken>>()
                .Where(kvp => !MetadataPropertyKey.Equals(kvp.Key))
                .Aggregate(new BasicStruct(new BasicStruct.Initializer(valueMetadata)), (@struct, jkvp) =>
                {
                    var newPath = MapAccessor.ToString().Equals(path)
                        ? $"{path}{jkvp.Key}"
                        : $"{path}{MapAccessor}{jkvp.Key}";

                    var (_, _, propertyMetadata) = ExtractMetadata(newPath, metadata);
                    var propertyName = new BasicStruct.PropertyName(jkvp.Key, propertyMetadata);

                    return @struct.AddValue(propertyName, ToBasicValue(jkvp.Value, newPath, metadata));
                });
        }

        private BasicList ToBasicList(JArray jarray, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            var (_, valueMetadata, _) = ExtractMetadata(path, metadata);

            return jarray
                .Select((jtoken, index) => ToBasicValue(jtoken, $"{path}{ArrayAccessor}{index}", metadata))
                .ApplyTo(values => IBasicValue.Of(values, valueMetadata))
                .As<BasicList>();
        }

        private IBasicValue ToBasicValue(JToken token, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            var (typeMetadata, valueMetadata, _) = ExtractMetadata(path, metadata);
            var parseInfo = ParseInfo ?? new DateTimeParseInfo();

            return typeMetadata
                .Map(t => ToBasicTypes(t.Key), () => ToBasicTypes(token.Type))
                .Map(basicTypes => basicTypes switch
                {
                    BasicTypes.Bool => IBasicValue.Of(token.Value<bool>(), valueMetadata),

                    BasicTypes.Bytes => IBasicValue.Of(Convert.FromBase64String(token.Value<string>()), valueMetadata),

                    BasicTypes.Date => IBasicValue.Of(
                        DateTimeOffset.ParseExact(
                            token.Value<string>(),
                            parseInfo.Formats,
                            parseInfo.CultureInfo.DateTimeFormat,
                            parseInfo.Styles),
                        valueMetadata),

                    BasicTypes.Decimal => IBasicValue.Of(token.Value<decimal>(), valueMetadata),

                    BasicTypes.Guid => IBasicValue.Of(Guid.Parse(token.Value<string>()), valueMetadata),

                    BasicTypes.Int => IBasicValue.Of(token.Value<int>(), valueMetadata),

                    BasicTypes.Real => IBasicValue.Of(token.Value<double>(), valueMetadata),

                    BasicTypes.String => IBasicValue.Of(token.Value<string>(), valueMetadata),

                    BasicTypes.TimeSpan => IBasicValue.Of(TimeSpan.Parse(token.Value<string>()), valueMetadata),

                    BasicTypes.Struct => ToBasicStruct(token as JObject, path, metadata),

                    BasicTypes.List => ToBasicList(token as JArray, path, metadata),

                    _ => throw new InvalidOperationException($"Invalid type: {basicTypes}")
                })
                .ValueOrDefault();
        }
        #endregion

        #region Write

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jobject = ToJObject((BasicStruct)value);
            jobject.WriteTo(writer);
        }

        public JObject ToJObject(BasicStruct @struct)
        {
            var metadata = new Dictionary<string, HashSet<JsonMetadata>>();
            var jobject = (JObject)ToJToken(@struct, $"{MapAccessor}", metadata);

            //add the metadata map to the root struct
            var metadataMap = metadata.Aggregate(new JObject(), (jobj, property) =>
            {
                jobj[property.Key] = property.Value
                    .Select(imeta => imeta.ToString())
                    .JoinUsing(" ");

                return jobj;
            });
            jobject[MetadataPropertyKey] = metadataMap;

            return jobject;
        }

        private JObject ToJObject(BasicStruct @struct, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            var jobject = @struct.Value.Aggregate(new JObject(), (jobj, property) =>
            {
                var newPath = path.Equals($"{MapAccessor}")
                    ? $"{path}{property.Name.Name}"
                    : $"{path}{MapAccessor}{property.Name.Name}";

                //add property name metadata to the dictionary
                property.Name.Metadata
                    .Select(JsonMetadata.ToPropertyMetadata)
                    .ForAll(nameMetadata => metadata
                        .GetOrAdd(newPath, key => new HashSet<JsonMetadata>())
                        .Add(nameMetadata));

                jobj[property.Name.Name] = ToJToken(property.Value, newPath, metadata);

                return jobj;
            });

            return jobject;
        }

        private JArray ToJArray(BasicList list, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            var index = -1;
            return list.Value.Aggregate(new JArray(), (array, value) =>
            {
                index++;
                var newPath = $"{path}{ArrayAccessor}{index}";

                //add value metadata to the dictionary
                value.Metadata
                    .Select(JsonMetadata.ToValueMetadata)
                    .ForAll(valueMetadata => metadata
                        .GetOrAdd(newPath, key => new HashSet<JsonMetadata>())
                        .Add(valueMetadata));

                array.Add(ToJToken(value, newPath, metadata));

                return array;
            });
        }

        private JToken ToJToken(IBasicValue basicValue, string path, Dictionary<string, HashSet<JsonMetadata>> metadata)
        {
            if (basicValue == default)
                return JValue.CreateNull();

            //add value's metadata to the dictionary
            basicValue.Metadata
                .Select(JsonMetadata.ToValueMetadata)
                .ForAll(valueMetadata => metadata
                    .GetOrAdd(path, key => new HashSet<JsonMetadata>())
                    .Add(valueMetadata));

            //create ValueType metadata for the appropriate types
            var valueTypeMetadata = basicValue.Type switch
            {
                BasicTypes.Bytes => JsonMetadata.ToValueTypeMetadata(nameof(BasicTypes.Bytes)),

                BasicTypes.Decimal => JsonMetadata.ToValueTypeMetadata(nameof(BasicTypes.Decimal)),

                BasicTypes.Date => JsonMetadata.ToValueTypeMetadata(nameof(BasicTypes.Date)),

                BasicTypes.TimeSpan => JsonMetadata.ToValueTypeMetadata(nameof(BasicTypes.TimeSpan)),

                BasicTypes.Guid => JsonMetadata.ToValueTypeMetadata(nameof(BasicTypes.Guid)),

                _ => (JsonMetadata?)null
            };

            _ = valueTypeMetadata
                .Map(vtm => metadata
                    .GetOrAdd(path, key => new HashSet<JsonMetadata>())
                    .Add(vtm));

            //return the token
            var parseInfo = ParseInfo ?? new DateTimeParseInfo();
            return basicValue.Type switch
            {
                BasicTypes.Bool => new JValue(((BasicBool)basicValue).Value),

                BasicTypes.Bytes => new JValue(Convert.ToBase64String(((BasicBytes)basicValue).Value)),

                BasicTypes.Int => new JValue(((BasicInt)basicValue).Value),

                BasicTypes.Decimal => new JValue(((BasicDecimal)basicValue).Value),

                BasicTypes.Real => new JValue(((BasicReal)basicValue).Value),

                BasicTypes.String => new JValue(((BasicString)basicValue).Value),

                BasicTypes.Guid => new JValue(((BasicGuid)basicValue).Value),

                BasicTypes.Date => new JValue(((BasicDate)basicValue).Value?.ToString(parseInfo.Formats[0])),

                BasicTypes.TimeSpan => new JValue(((BasicTimeSpan)basicValue).Value),

                BasicTypes.List => ToJArray((BasicList)basicValue, path, metadata),

                BasicTypes.Struct => ToJObject((BasicStruct)basicValue, path, metadata),

                _ => throw new ArgumentException($"Invalid BasicType: {basicValue.Type}"),
            };
        }

        #endregion

        private static BasicTypes ToBasicTypes(string typeName)
        {
            if (Enum.TryParse<BasicTypes>(typeName, out var dataType))
                return dataType;

            else
                throw new ArgumentException($"Invalid {nameof(typeName)}: {typeName}");
        }

        private static BasicTypes ToBasicTypes(JTokenType tokenType)
        {
            return tokenType switch
            {
                JTokenType.Object => BasicTypes.Struct,
                JTokenType.Array => BasicTypes.List,
                JTokenType.Integer => BasicTypes.Int,
                JTokenType.Float => BasicTypes.Real,
                JTokenType.String => BasicTypes.String,
                JTokenType.Boolean => BasicTypes.Bool,
                JTokenType.Date => BasicTypes.Date,
                JTokenType.Bytes => BasicTypes.Bytes,
                JTokenType.Guid => BasicTypes.Guid,
                JTokenType.TimeSpan => BasicTypes.TimeSpan,
                _ => BasicTypes.String,
            };
        }

        private static KeyValuePair<string, HashSet<JsonMetadata>> ToJsonMetadataPair(KeyValuePair<string, JToken> tokenPair)
        {
            return tokenPair.Key.ValuePair(
                new HashSet<JsonMetadata>(
                    JsonMetadata.ParseCollection(tokenPair.Value.Value<string>())));
        }

        private static Dictionary<string, HashSet<JsonMetadata>> ExtractMetadata(JObject jobj)
        {
            return jobj?
                .As<IEnumerable<KeyValuePair<string, JToken>>>()
                .Select(ToJsonMetadataPair)
                .ToDictionary()
                ?? new Dictionary<string, HashSet<JsonMetadata>>();
        }

        /// <summary>
        /// Extract metadata group for the given key
        /// </summary>
        /// <param name="path">The path-key</param>
        /// <param name="metadataMap">The metadtaa map</param>
        /// <returns>The <c>ValueType</c>, <c>Value</c>, and <c>PropertyName</c> metadata, respectively.</returns>
        private static (Metadata?, Metadata[], Metadata[]) ExtractMetadata(
            string path,
            Dictionary<string, HashSet<JsonMetadata>> metadataMap)
        {
            if (!metadataMap.TryGetValue(path, out var metadataCollection))
                return (null, Array.Empty<Metadata>(), Array.Empty<Metadata>());

            //Extract metadata
            var value = new List<Metadata>();
            var propertyName = new List<Metadata>();
            Metadata? type = null;

            metadataCollection.ForAll(meta =>
            {
                switch (meta.Symbol)
                {
                    case MetadataSymbols.ValueTypeMetadata:
                        type = meta.BasicMetadata;
                        break;

                    case MetadataSymbols.ValueMetadata:
                        value.Add(meta.BasicMetadata);
                        break;

                    case MetadataSymbols.PropertyNameMetadata:
                        propertyName.Add(meta.BasicMetadata);
                        break;

                    default: throw new Exception($"Invalid metadata symbol: {meta.Symbol}");
                }
            });

            return (type, value.ToArray(), propertyName.ToArray());
        }

        #region Nested Types

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

        internal enum MetadataSymbols
        {
            ValueMetadata = '#',
            PropertyNameMetadata = '$',
            ValueTypeMetadata = '@'
        }

        internal struct JsonMetadata
        {
            private readonly Metadata _metadata;

            public string Key => _metadata.Key;

            public string Value => _metadata.Value;

            public MetadataSymbols Symbol { get; }

            public Metadata BasicMetadata => _metadata;


            public JsonMetadata(MetadataSymbols symbol, Metadata metadata)
            {
                _metadata = metadata;
                Symbol = symbol;
            }

            public override string ToString() => $"{Symbol.Char()}{_metadata}";

            public override int GetHashCode() => HashCode.Combine(Symbol, _metadata.GetHashCode());

            public override bool Equals(object obj)
            {
                return obj is JsonMetadata other
                    && other._metadata.Equals(_metadata)
                    && other.Symbol.Equals(Symbol);
            }


            public static JsonMetadata Parse(string metadataString)
            {
                if (string.IsNullOrEmpty(metadataString))
                    return default;

                var @string = metadataString.Trim();
                var symbol = (MetadataSymbols)@string[0];
                return Parse(symbol, @string[1..]);
            }

            private static JsonMetadata Parse(
                MetadataSymbols symbol,
                string metadataString)
                => new JsonMetadata(symbol, Metadata.Parse(metadataString));

            /// <summary>
            /// Splits the string using ';'. This means the individual metadata 'Parse' methods will receive their string sans the ';' at the end.
            /// </summary>
            /// <param name="metadataCollectionString">The metadata string</param>
            /// <returns>the array of metadata parsed from the string</returns>
            public static JsonMetadata[] ParseCollection(string metadataCollectionString)
            {
                return metadataCollectionString?
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Parse)
                    .ToArray()
                    ?? Array.Empty<JsonMetadata>();
            }

            public static JsonMetadata ToPropertyMetadata(Metadata metadata)
                => new JsonMetadata(
                    MetadataSymbols.PropertyNameMetadata,
                    metadata);
            public static JsonMetadata ToValueMetadata(Metadata metadata)
                => new JsonMetadata(
                    MetadataSymbols.ValueMetadata,
                    metadata);
            public static JsonMetadata ToValueTypeMetadata(Metadata metadata)
                => new JsonMetadata(
                    MetadataSymbols.ValueTypeMetadata,
                    metadata);
        }

        #endregion
    }
}
