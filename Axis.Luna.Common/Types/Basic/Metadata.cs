using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;

namespace Axis.Luna.Common.Types.Basic
{

    /// <summary>
    /// Key value pair formated like css properties: <c>key-1:value1;</c>
    /// </summary>
    public readonly struct Metadata
    {
        public string Key { get; }
        public string Value { get; }

        public Metadata(string key, string value = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
        }

        public Metadata(KeyValuePair<string, string> metadata)
            : this(metadata.Key, metadata.Value)
        { }

        public Metadata((string key, string value) metadata)
            : this(metadata.key, metadata.value)
        { }

        public override string ToString()
        {
            if (this == default)
                return "";

            if (string.IsNullOrEmpty(Value))
                return $"{Key};";

            return $"{Key}:{Value};";
        }

        public override bool Equals(object obj)
        {
            return obj is Metadata other
                && other.Key.NullOrEquals(Key)
                && other.Value.NullOrEquals(Value);
        }

        public override int GetHashCode() => HashCode.Combine(Key, Value);

        public static bool TryParse(string value, out Metadata metadata)
        {
            if (TryParse(value, out IResult<Metadata> result))
            {
                metadata = result
                    .As<IResult<Metadata>.DataResult>()
                    .Data;
                return true;
            }

            metadata = default;
            return false;
        }

        public static Metadata Parse(string @string)
        {
            _ =  TryParse(@string, out IResult<Metadata> result);
            return result.Resolve();
        }

        private static bool TryParse(string @string, out IResult<Metadata> result)
        {
            if (@string == null)
            {
                result = Result.Of<Metadata>(new ArgumentNullException(nameof(@string)));
                return false;
            }

            var parts = @string
                .Trim()
                .TrimEnd(';')
                .Split(':');

            if (parts.Length < 1 || parts.Length > 2)
            {
                result = Result.Of<Metadata>(new FormatException($"Invalid metadata format: {@string}"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                result = Result.Of<Metadata>(new FormatException($"metadata key cannot be null or whitespace"));
                return false;
            }

            result = Result.Of(new Metadata(parts[0].Trim(), parts.Length > 1 ? parts[1] : null));
            return true;
        }

        public static bool operator ==(Metadata first, Metadata second) => first.Equals(second);

        public static bool operator !=(Metadata first, Metadata second) => !first.Equals(second);

        public static implicit operator Metadata(string value)
        {
            if (!TryParse(value, out IResult<Metadata> result))
                throw result
                    .As<IResult<Metadata>.ErrorResult>()
                    .Cause();

            else return result
                    .As<IResult<Metadata>.DataResult>()
                    .Data;
        }

        public static Metadata[] Empty => Array.Empty<Metadata>();
    }
}
