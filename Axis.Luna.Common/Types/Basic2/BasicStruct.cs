using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic2
{
    public partial interface IBasicValue
    {
        public struct BasicStruct : IBasicValue
        {
            private readonly Metadata[] _metadata;
            private readonly Dictionary<PropertyName, IBasicValue> _properties;
            private readonly Dictionary<string, PropertyName> _propertyNames;

            #region Properties
            public BasicTypes Type => BasicTypes.Struct;

            public Metadata[] Metadata => _metadata?.ToArray() ?? Array.Empty<Metadata>();

            public bool IsDefault => _properties is null;

            public int PropertyCount => _properties?.Count ?? 0;

            public bool ContainsProperty(string name) => _propertyNames?.ContainsKey(name) == true;

            public PropertyName[] PropertyNames => _properties
                ?.Keys
                .ToArray()
                ?? Array.Empty<PropertyName>();

            public Property[] Properties => _properties
                ?.Select(p => new Property(p.Key, p.Value))
                .ToArray()
                ?? Array.Empty<Property>();
            #endregion

            #region Indexer
            public IBasicValue this[PropertyName propertyName]
            {
                get => TryGetValue(propertyName.Name, out var value)
                    ? value
                    : throw new ArgumentException($"Invalid property name");

                set => _ = AddValue(propertyName, value);
            }

            public IBasicValue this[string propertyName]
            {
                get => TryGetValue(propertyName, out var value)
                    ? value
                    : throw new ArgumentException($"Invalid property name");

                set => _ = AddValue(propertyName, value);
            }
            #endregion

            #region Ctor
            internal BasicStruct(Property[] initialProperties, params Metadata[] metadata)
            {
                _metadata = metadata?.ToArray() ?? Array.Empty<Metadata>();
                (_propertyNames, _properties) = initialProperties
                    .ThrowIfNull(new ArgumentNullException(nameof(initialProperties)))
                    .Aggregate(
                        (propNames: new Dictionary<string, PropertyName>(), props: new Dictionary<PropertyName, IBasicValue>()),
                        (maps, next) =>
                        {
                            maps.propNames[next.Name.Name] = next.Name;
                            maps.props[next.Name] = next.Value;

                            return maps;
                        });
            }

            public BasicStruct(params Metadata[] metadata)
                : this(Array.Empty<Property>(), metadata)
            { }
            #endregion

            #region Accessors
            public bool TryGetValue(string propertyName, out IBasicValue value)
            {
                ValidateDefault();

                value = null;

                if (!_propertyNames.TryGetValue(propertyName, out var name))
                    return false;

                value = _properties[name];
                return true;
            }

            public IBasicValue GetOrAddValue(string propertyName, Func<string, IBasicValue> valueProducer)
            {
                ValidateDefault();

                if (valueProducer == null)
                    throw new ArgumentNullException(nameof(valueProducer));

                if (TryGetValue(propertyName, out var value))
                    return value;

                else
                {
                    _ = AddValue(propertyName, valueProducer.Invoke(propertyName));
                    return _properties[_propertyNames[propertyName]];
                }
            }

            public IBasicValue GetOrAddValue(PropertyName propertyName, Func<PropertyName, IBasicValue> valueProducer)
            {
                ValidateDefault();

                if (propertyName == default)
                    throw new ArgumentException($"Invalid {nameof(propertyName)}");

                if (valueProducer == null)
                    throw new ArgumentNullException(nameof(valueProducer));

                if (TryGetValue(propertyName.Name, out var value))
                    return value;

                else
                {
                    _ = AddValue(propertyName, valueProducer.Invoke(propertyName));
                    return _properties[propertyName];
                }
            }
            #endregion

            #region Mutators
            public BasicStruct AddValue(string propertyName, IBasicValue value)
            {
                ValidateDefault();

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                var propName = _propertyNames.GetOrAdd(propertyName, n => new PropertyName(n));
                _properties[propName] = value;

                return this;
            }

            public BasicStruct AddValue(PropertyName propertyName, IBasicValue value)
            {
                ValidateDefault();

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (propertyName == default)
                    throw new ArgumentException($"Invalid {nameof(propertyName)}");

                _propertyNames[propertyName.Name] = propertyName;
                _properties[propertyName] = value;

                return this;
            }

            public bool TryRemove(string propertyName, out IBasicValue value)
            {
                ValidateDefault();

                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new ArgumentException($"Invalid {nameof(propertyName)}");

                value = null;
                if (!_propertyNames.Remove(propertyName, out var propName))
                    return false;

                _properties.Remove(propName, out value);
                return true;
            }

            public BasicStruct Remove(string propertyName)
            {
                ValidateDefault();

                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new ArgumentException($"Invalid {nameof(propertyName)}");

                _ = TryRemove(propertyName, out _);

                return this;
            }
            #endregion

            #region Utils
            private void ValidateDefault()
                => this.ThrowIfDefault(
                    new InvalidOperationException($"The operation is invalid on a default {nameof(BasicStruct)}"));
            #endregion

            #region Record implementation

            public override int GetHashCode()
            {
                if (IsDefault)
                    return 0;

                var keyHash = Luna.Extensions.Common.ValueHash(
                    _properties?.Keys.HardCast<PropertyName, object>());

                var valueHash = Luna.Extensions.Common.ValueHash(
                    _properties?.Values.HardCast<IBasicValue, object>());

                return HashCode.Combine(keyHash, valueHash);
            }

            private bool ContainsAllPropertiesOf(BasicStruct other)
            {
                var @this = this;
                return _properties
                    ?.All(prop =>
                    {
                        if (!other.TryGetValue(prop.Key.Name, out var value))
                            return false;

                        else
                            return prop.Value.Equals(value);
                    })
                    ?? false;
            }

            public bool EquivalentTo(BasicStruct other)
            {
                if (this.IsDefault && other.IsDefault)
                    return true;

                if (PropertyCount != other.PropertyCount)
                    return false;

                if (!ContainsAllPropertiesOf(other))
                    return false;

                else return true;
            }

            public bool ExactyCopyOf(BasicStruct other)
            {
                return _metadata == other._metadata
                    && _properties == other._properties
                    && _propertyNames == other._propertyNames;
            }

            public override bool Equals(object obj)
            {
                return obj is BasicStruct other
                    && (ExactyCopyOf(other) || EquivalentTo(other));
            }


            public static bool operator ==(BasicStruct first, BasicStruct second) => first.Equals(second);

            public static bool operator !=(BasicStruct first, BasicStruct second) => !first.Equals(second);


            public static implicit operator BasicStruct(Initializer value) 
                => new BasicStruct(
                    value.Properties.ToArray(),
                    value.Metadata);

            #endregion

            #region Types

            /// <summary>
            /// Represents a property name for a <see cref="BasicStruct"/>.
            /// <para>
            /// There are no restrictions on naming conventions for struct property names, except that the <see cref="Basic2.Metadata"/> may also be encoded
            /// in the string at the start of the string, and enclosed in '[...]'. See <see cref="Metadata.Parse(string)"/> for the format
            /// of each individual metadata.
            /// </para>
            /// </summary>
            public readonly struct PropertyName
            {
                private readonly Metadata[] _metadata;

                public string Name { get; }

                public Metadata[] Metadata => _metadata?.ToArray();


                public PropertyName(string name, params Metadata[] metadata)
                {
                    Name = name ?? throw new ArgumentException(nameof(name));
                    _metadata = metadata?.ToArray() ?? Array.Empty<Metadata>();
                }

                public override int GetHashCode()
                    => HashCode.Combine(
                        Name,
                        Luna.Extensions.Common.ValueHash(_metadata.HardCast<Metadata, object>()));

                public override bool Equals(object obj)
                {
                    return obj is PropertyName other
                        && other.Name.NullOrEquals(Name)
                        && other.Metadata.NullOrTrue(
                            Metadata,
                            Enumerable.SequenceEqual);
                }

                public override string ToString()
                {
                    return Metadata
                        .Select(metadata => metadata.ToString())
                        .JoinUsing("")
                        .WrapIn("[", "]")
                        + Name;
                }

                public static PropertyName Parse(string @string)
                {
                    if (TryParse(@string, out IResult<PropertyName> result))
                        return result.As<IResult<PropertyName>.DataResult>().Data;

                    else throw result.As<IResult<PropertyName>.ErrorResult>().Cause();
                }

                public static bool TryParse(string @string, out PropertyName value)
                {
                    if (TryParse(@string, out IResult<PropertyName> result))
                    {
                        value = result.As<IResult<PropertyName>.DataResult>().Data;
                        return true;
                    }

                    value = default;
                    return false;
                }

                private static bool TryParse(string @string, out IResult<PropertyName> result)
                {
                    if(string.IsNullOrWhiteSpace(@string))
                    {
                        result = IResult<PropertyName>.Of(new ArgumentException($"Invalid {nameof(@string)}"));
                        return false;
                    }

                    Metadata[] metadatalist = null;
                    string propertyName = null;
                    if (@string.StartsWith("["))
                    {
                        var closingBracketIndex = @string.IndexOf(']');
                        if (closingBracketIndex < 0)
                        {
                            result = IResult<PropertyName>.Of(new FormatException($"Invalid {nameof(@string)}. Expected ']'"));
                            return false;
                        }

                        metadatalist = @string[..(closingBracketIndex + 1)]
                            .UnwrapFrom("[", "]")
                            .Split(';', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Basic2.Metadata.Parse)
                            .ToArray();

                        propertyName = @string.Length > closingBracketIndex + 1
                            ? @string[(closingBracketIndex + 1)..].Trim()
                            : "";
                    }
                    else metadatalist = Array.Empty<Metadata>();

                    propertyName ??= @string.Trim();
                    if(string.IsNullOrWhiteSpace(propertyName))
                    {
                        result = IResult<PropertyName>.Of(new FormatException($"Invalid {nameof(@string)}. Expected a property name."));
                        return false;
                    }

                    result = IResult<PropertyName>.Of(new PropertyName(propertyName, metadatalist));
                    return true;
                }

                public static bool operator ==(PropertyName first, PropertyName second) => first.Equals(second);

                public static bool operator !=(PropertyName first, PropertyName second) => !first.Equals(second);

                public static implicit operator PropertyName(string value) => Parse(value);
            }

            /// <summary>
            /// Represents a simple name-value combination of a <see cref="PropertyName"/> and a <see cref="IBasicValue"/>.
            /// </summary>
            public class Property
            {                
                public PropertyName Name { get; }

                public IBasicValue Value { get; set; }


                public Property(PropertyName name, IBasicValue value = null)
                {
                    Name = name.ThrowIfDefault(new ArgumentException($"Invalid {nameof(name)} supplied"));
                    Value = value;
                }
            }

            /// <summary>
            /// This type wraps around <see cref="IBasicValue"/> instances as a convenient way for them to be implicitly converted from their native values,
            /// while assigning them via the indexer of a <see cref="BasicStruct"/> instance.
            /// </summary>
            public readonly struct BasicValueWrapper
            {
                public IBasicValue Value { get; }

                internal BasicValueWrapper(IBasicValue value)
                {
                    Value = value;
                }

                #region implicits

                #region ints
                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(sbyte? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(sbyte value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(short? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(short value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(int? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(int value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(long? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(long value) => new BasicValueWrapper(Of(value));
                #endregion

                #region uints
                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(byte? value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(byte value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(ushort? value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(ushort value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(uint? value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(uint value) => new BasicValueWrapper(Of((ulong?)value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(ulong? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(ulong value) => new BasicValueWrapper(Of(value));
                #endregion

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(double? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(decimal? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(bool? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(string value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(DateTimeOffset? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(TimeSpan? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(Guid? value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(byte[] value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(IBasicValue[] value) => new BasicValueWrapper(Of(value));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(BasicValueWrapper[] value) 
                    => new BasicValueWrapper(Of(value?.Select(v => v.Value)));

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(BasicStruct value) => new BasicValueWrapper(value);

                /// <summary>
                /// 
                /// </summary>
                /// <param name="value"></param>
                public static implicit operator BasicValueWrapper(Initializer value) => new BasicValueWrapper(Of(value?.Properties, value?.Metadata));

                #endregion
            }


            /// <summary>
            /// Initializer for <see cref="BasicStruct"/>
            /// </summary>
            public class Initializer : IWriteonlyIndexer<PropertyName, BasicValueWrapper>
            {
                private readonly Dictionary<PropertyName, BasicValueWrapper> _map = new Dictionary<PropertyName, BasicValueWrapper>();
                private readonly Metadata[] _metadata;

                public Initializer(params Metadata[] metadata)
                {
                    _metadata = metadata ?? Array.Empty<Metadata>();
                }

                public Initializer()
                    :this(Array.Empty<Metadata>())
                {
                }

                internal Dictionary<PropertyName, BasicValueWrapper> Map => _map;

                internal Property[] Properties => _map
                    .Select(kvp => new Property(kvp.Key, kvp.Value.Value))
                    .ToArray();

                internal Metadata[] Metadata => _metadata ?? Array.Empty<Metadata>();

                public BasicValueWrapper this[PropertyName key]
                {
                    set => _map[key] = value;
                }
            }
            #endregion
        }
    }
}
