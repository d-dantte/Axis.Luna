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

            public IBasicValue this[string propertyName]
            {
                get => TryGetValue(propertyName, out var value)
                    ? value
                    : null;

                set => _ = AddValue(propertyName, value);
            }

            public IBasicValue this[PropertyName propertyName]
            {
                get => TryGetValue(propertyName.Name, out var value)
                    ? value
                    : null;

                set => _ = AddValue(propertyName, value);
            }

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
                    return this[propertyName];
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
                    return this[propertyName];
                }
            }
            #endregion

            #region Mutators
            public BasicStruct AddValue(string propertyName, IBasicValue value)
            {
                ValidateDefault();

                var propName = _propertyNames.GetOrAdd(propertyName, n => new PropertyName(n));
                _properties[propName] = value;

                return this;
            }

            public BasicStruct AddValue(PropertyName propertyName, IBasicValue value)
            {
                ValidateDefault();

                _propertyNames[propertyName.Name] = propertyName;
                _properties[propertyName] = value;

                return this;
            }

            public bool TryRemove(string propertyName, out IBasicValue value)
            {
                ValidateDefault();

                value = null;
                if (!_propertyNames.Remove(propertyName, out var propName))
                    return false;

                _properties.Remove(propName, out value);
                return true;
            }

            public BasicStruct Remove(string propertyName)
            {
                ValidateDefault();

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
                    _properties?.Keys.ToArray());

                var valueHash = Luna.Extensions.Common.ValueHash(
                    _properties?.Values.ToArray());

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
            #endregion

            #region Types
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
                        Luna.Extensions.Common.ValueHash(_metadata));

                public override bool Equals(object obj)
                {
                    return obj is PropertyName other
                        && other.Name.NullOrEquals(Name)
                        && other.Metadata.NullOrTrue(
                            Metadata,
                            Enumerable.SequenceEqual);
                }

                public static bool operator ==(PropertyName first, PropertyName second) => first.Equals(second);

                public static bool operator !=(PropertyName first, PropertyName second) => !first.Equals(second);
            }

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
            #endregion
        }
    }
}
