using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using static Axis.Luna.Extensions.ExceptionExtension;
using static Axis.Luna.Common.NewtonsoftJson.Extensions;

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

        private BasicStruct ToBasicStruct(JObject jobject, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            var (_, valueMetadata, _) = ExtractMetadata(path, metadata);

            return jobject
                .As<IDictionary<string, JToken>>()
                .Where(kvp => !MetadataPropertyKey.Equals(kvp.Key))
                .Aggregate(new BasicStruct(valueMetadata), (@struct, jkvp) =>
                {
                    var newPath = MapAccessor.ToString().Equals(path)
                        ? $"{path}{jkvp.Key}"
                        : $"{path}{MapAccessor}{jkvp.Key}";

                    var (_, _, propertyMetadata) = ExtractMetadata(newPath, metadata);
                    var propertyName = new BasicStruct.PropertyName(jkvp.Key, propertyMetadata);

                    return @struct.Append(propertyName, ToBasicValue(jkvp.Value, newPath, metadata));
                });
        }

        private BasicList ToBasicList(JArray jarray, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            var (_, valueMetadata, _) = ExtractMetadata(path, metadata);

            return jarray
                .Select((jtoken, index) => ToBasicValue(jtoken, $"{path}{ArrayAccessor}{index}", metadata))
                .ApplyTo(values => new BasicList(values, valueMetadata));
        }

        private BasicValue ToBasicValue(JToken token, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            var (typeMetadata, valueMetadata, _) = ExtractMetadata(path, metadata);
            var parseInfo = ParseInfo ?? new DateTimeParseInfo();

            return typeMetadata
                .Map(t => ToBasicTypes(t.Key), () => ToBasicTypes(token.Type))
                .Map(basicTypes => basicTypes switch
                {
                    BasicTypes.Bool => new BasicBool(token.Value<bool>(), valueMetadata),

                    BasicTypes.Bytes => new BasicBytes(Convert.FromBase64String(token.Value<string>()), valueMetadata),

                    BasicTypes.Date => new BasicDateTime(
                        DateTimeOffset.ParseExact(
                            token.Value<string>(),
                            parseInfo.Formats,
                            parseInfo.CultureInfo.DateTimeFormat,
                            parseInfo.Styles),
                        valueMetadata),

                    BasicTypes.Decimal => new BasicDecimal(token.Value<decimal>(), valueMetadata),

                    BasicTypes.Guid => new BasicGuid(Guid.Parse(token.Value<string>()), valueMetadata),

                    BasicTypes.Int => new BasicInt(token.Value<int>(), valueMetadata),

                    BasicTypes.Real => new BasicReal(token.Value<double>(), valueMetadata),

                    BasicTypes.String => new BasicString(token.Value<string>(), valueMetadata),

                    BasicTypes.TimeSpan => new BasicTimeSpan(TimeSpan.Parse(token.Value<string>()), valueMetadata),

                    BasicTypes.Struct => ToBasicStruct(token as JObject, path, metadata),

                    BasicTypes.List => ToBasicList(token as JArray, path, metadata),

                    _ => new BasicValue(new BasicString(null))
                })
                .Value;
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
            var metadata = new Dictionary<string, HashSet<IMetadata>>();
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

        private JObject ToJObject(BasicStruct @struct, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            var jobject = @struct.Value.Aggregate(new JObject(), (jobj, property) =>
            {
                var newPath = path.Equals($"{MapAccessor}")
                    ? $"{path}{property.Key.Name}"
                    : $"{path}{MapAccessor}{property.Key.Name}";

                //add property name metadata to the dictionary
                property.Key.Metadata
                    .Select(PropertyNameMetadata.FromBasic)
                    .ForAll(nameMetadata => metadata
                        .GetOrAdd(newPath, key => new HashSet<IMetadata>())
                        .Add(nameMetadata));

                jobj[property.Key.Name] = ToJToken(property.Value, newPath, metadata);

                return jobj;
            });

            return jobject;
        }

        private JArray ToJArray(BasicList list, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            var index = -1;
            return list.Value.Aggregate(new JArray(), (array, value) =>
            {
                index++;
                var newPath = $"{path}{ArrayAccessor}{index}";

                //add value metadata to the dictionary
                value.Metadata
                    .Select(ValueMetadata.FromBasic)
                    .ForAll(valueMetadata => metadata
                        .GetOrAdd(newPath, key => new HashSet<IMetadata>())
                        .Add(valueMetadata));

                array.Add(ToJToken(value, newPath, metadata));

                return array;
            });
        }

        private JToken ToJToken(BasicValue basicValue, string path, Dictionary<string, HashSet<IMetadata>> metadata)
        {
            if (basicValue == default)
                return JValue.CreateNull();

            //add value's metadata to the dictionary
            basicValue.Metadata
                .Select(ValueMetadata.FromBasic)
                .ForAll(valueMetadata => metadata
                    .GetOrAdd(path, key => new HashSet<IMetadata>())
                    .Add(valueMetadata));

            //create ValueType metadata for the appropriate types
            var valueTypeMetadata = basicValue.Type switch
            {
                BasicTypes.Bytes => new ValueTypeMetadata(nameof(BasicTypes.Bytes)),

                BasicTypes.Decimal => new ValueTypeMetadata(nameof(BasicTypes.Decimal)),

                BasicTypes.Date => new ValueTypeMetadata(nameof(BasicTypes.Date)),

                BasicTypes.TimeSpan => new ValueTypeMetadata(nameof(BasicTypes.TimeSpan)),

                BasicTypes.Guid => new ValueTypeMetadata(nameof(BasicTypes.Guid)),

                _ => (ValueTypeMetadata?)null
            };

            _ = valueTypeMetadata
                .Map(vtm => metadata
                    .GetOrAdd(path, key => new HashSet<IMetadata>())
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

                BasicTypes.Date => new JValue(((BasicDateTime)basicValue).Value?.ToString(parseInfo.Formats[0])),

                BasicTypes.TimeSpan => new JValue(((BasicTimeSpan)basicValue).Value),

                BasicTypes.List => ToJArray((BasicList)basicValue, path, metadata),

                BasicTypes.Struct => ToJObject((BasicStruct)basicValue, path, metadata),

                BasicTypes.NullValue => JValue.CreateNull(),

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
                JTokenType.Null => BasicTypes.NullValue,
                _ => BasicTypes.String,
            };
        }

        private static KeyValuePair<string, HashSet<IMetadata>> ToIMetadataPair(KeyValuePair<string, JToken> tokenPair)
        {
            return tokenPair.Key.ValuePair(
                new HashSet<IMetadata>(
                    MetadataParser.ParseMetadata(tokenPair.Value.Value<string>())));
        }

        private static Dictionary<string, HashSet<IMetadata>> ExtractMetadata(JObject jobj)
        {
            return jobj?
                .As<IEnumerable<KeyValuePair<string, JToken>>>()
                .Select(ToIMetadataPair)
                .ToDictionary()
                ?? new Dictionary<string, HashSet<IMetadata>>();
        }

        /// <summary>
        /// Extract metadata group for the given key
        /// </summary>
        /// <param name="path">The path-key</param>
        /// <param name="metadataMap">The metadtaa map</param>
        /// <returns>The <c>ValueType</c>, <c>Value</c>, and <c>PropertyName</c> metadata, respectively.</returns>
        private static (BasicMetadata?, BasicMetadata[], BasicMetadata[]) ExtractMetadata(
            string path,
            Dictionary<string, HashSet<IMetadata>> metadataMap)
        {
            if (!metadataMap.TryGetValue(path, out var metadataCollection))
                return (null, Array.Empty<BasicMetadata>(), Array.Empty<BasicMetadata>());

            //Extract metadata
            List<BasicMetadata> value = new List<BasicMetadata>(), propertyName = new List<BasicMetadata>();
            BasicMetadata? type = null;

            metadataCollection.ForAll(meta =>
            {
                switch (meta.Symbol)
                {
                    case MetadataSymbols.ValueTypeMetadata:
                        type = meta.ToBasicMetadata();
                        break;

                    case MetadataSymbols.ValueMetadata:
                        value.Add(meta.ToBasicMetadata());
                        break;

                    case MetadataSymbols.PropertyNameMetadata:
                        propertyName.Add(meta.ToBasicMetadata());
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

        #region Metadata

        internal enum MetadataSymbols
        {
            ValueMetadata = '#',
            PropertyNameMetadata = '$',
            ValueTypeMetadata = '@'
        }

        internal interface IMetadata
        {
            string Key { get; }
            string Value { get; }
            MetadataSymbols Symbol { get; }

            string ToString();
        }

        internal struct PropertyNameMetadata : IMetadata
        {
            public MetadataSymbols Symbol => MetadataSymbols.PropertyNameMetadata;

            public string Key { get; }

            public string Value { get; }

            public PropertyNameMetadata(string key, string value = null)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Value = value;
            }

            public override bool Equals(object obj)
            {
                return obj is PropertyNameMetadata other
                    && other.Key.Equals(Key)
                    && other.Value.NullOrEquals(Value);
            }

            public override int GetHashCode() => HashCode.Combine(Key, Value);

            public override string ToString()
            {
                var v = Value != null ? $":{Value}" : null;
                return $"{Symbol.Char()}{Key}{v};";
            }

            /// <summary>
            /// Parse raw string into a <see cref="PropertyNameMetadata"/>. Note that the input string MUST begin with <see cref="PropertyNameMetadata.Symbol"/>
            /// </summary>
            /// <param name="data">the raw string</param>
            /// <returns>the parsed metadata</returns>
            public static PropertyNameMetadata Parse(string data)
            {
                if (string.IsNullOrWhiteSpace(data) || data[0] != MetadataSymbols.PropertyNameMetadata.Char())
                    throw new ArgumentException($"Invalid '{nameof(data)}' argument");

                var parts = data
                    .TrimStart(MetadataSymbols.PropertyNameMetadata.Char())
                    .TrimEnd(';', ' ')
                    .Split(':');

                if(parts.Length < 1 || parts.Length > 2 || parts.Any(p => p == string.Empty))
                    throw new ArgumentException($"Invalid '{nameof(data)}' argument");

                return new PropertyNameMetadata(
                    parts[0].Trim(),
                    parts.Length == 2 ? parts[1] : null);
            }

            public static PropertyNameMetadata FromBasic(BasicMetadata metadata) => new PropertyNameMetadata(metadata.Key, metadata.Value);
        }

        internal struct ValueMetadata : IMetadata
        {
            public MetadataSymbols Symbol => MetadataSymbols.ValueMetadata;

            public string Key { get; }

            public string Value { get; }

            public ValueMetadata(string key, string value = null)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                Value = value;
            }

            public override bool Equals(object obj)
            {
                return obj is ValueMetadata other
                    && other.Key.Equals(Key)
                    && other.Value.NullOrEquals(Value);
            }

            public override int GetHashCode() => HashCode.Combine(Key, Value);

            public override string ToString()
            {
                var v = Value != null ? $":{Value}" : null;
                return $"{Symbol.Char()}{Key}{v};";
            }


            /// <summary>
            /// Parse raw string into a <see cref="ValueMetadata"/>. Note that the input string MUST begin with <see cref="ValueMetadata.Symbol"/>
            /// </summary>
            /// <param name="data">the raw string</param>
            /// <returns>the parsed metadata</returns>
            public static ValueMetadata Parse(string data)
            {
                if (string.IsNullOrWhiteSpace(data) || data[0] != MetadataSymbols.ValueMetadata.Char())
                    throw new ArgumentException($"Invalid '{nameof(data)}' argument");

                var parts = data
                    .TrimStart(MetadataSymbols.ValueMetadata.Char())
                    .TrimEnd(';', ' ')
                    .Split(':');

                if (parts.Length < 1 || parts.Length > 2 || parts.Any(p => p == string.Empty))
                    throw new ArgumentException($"Invalid '{nameof(data)}' argument");

                return new ValueMetadata(
                    parts[0].Trim(),
                    parts.Length == 2 ? parts[1] : null);
            }

            public static ValueMetadata FromBasic(BasicMetadata metadata) => new ValueMetadata(metadata.Key, metadata.Value);
        }

        internal struct ValueTypeMetadata: IMetadata
        {
            public MetadataSymbols Symbol => MetadataSymbols.ValueTypeMetadata;

            public string Key { get; }

            public string Value { get; }

            public ValueTypeMetadata(string typeName)
            {
                Value = null;
                Key = typeName.ThrowIf(
                    string.IsNullOrWhiteSpace,
                    new ArgumentNullException(nameof(typeName)));
            }

            public override bool Equals(object obj)
            {
                return obj is ValueTypeMetadata other
                    && other.Key.NullOrEquals(Key);
            }

            public override int GetHashCode() => HashCode.Combine(Key);

            public override string ToString()
            {
                return $"{Symbol.Char()}{Key};";
            }


            /// <summary>
            /// Parse raw string into a <see cref="ValueTypeMetadata"/>. Note that the input string MUST begin with <see cref="ValueTypeMetadata.Symbol"/>
            /// </summary>
            /// <param name="data">the raw string</param>
            /// <returns>the parsed metadata</returns>
            public static ValueTypeMetadata Parse(string data)
            {
                if (string.IsNullOrWhiteSpace(data) || data[0] != MetadataSymbols.ValueTypeMetadata.Char())
                    throw new ArgumentException($"Invalid '{nameof(data)}' argument");

                var type = data
                    .TrimStart(MetadataSymbols.ValueTypeMetadata.Char())
                    .TrimEnd(' ', ';');

                if (string.IsNullOrWhiteSpace(type))
                    throw new ArgumentException($"Invalid type metadata: {data}");

                return new ValueTypeMetadata(type);
            }
        }

        internal static class MetadataParser
        {
            public static IMetadata Parse(string metadataString)
            {
                if (string.IsNullOrEmpty(metadataString))
                    return null;

                return (MetadataSymbols)metadataString[0] switch
                {
                    MetadataSymbols.PropertyNameMetadata => PropertyNameMetadata.Parse(metadataString),
                    MetadataSymbols.ValueMetadata => ValueMetadata.Parse(metadataString),
                    MetadataSymbols.ValueTypeMetadata => ValueTypeMetadata.Parse(metadataString),

                    _ => throw new ArgumentException($"Invalid metadata symbol: {metadataString[0]}")
                };
            }

            /// <summary>
            /// Splits the string using ';'. This means the individual metadata 'Parse' methods will receive their string sans the ';' at the end.
            /// </summary>
            /// <param name="metadataCollectionString">The metadata string</param>
            /// <returns>the array of metadata parsed from the string</returns>
            public static IMetadata[] ParseMetadata(string metadataCollectionString)
            {
                return metadataCollectionString?
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => Parse(v.Trim()))
                    .ToArray()
                    ?? Array.Empty<IMetadata>();
            }
        }
        #endregion

        #endregion
    }
}
