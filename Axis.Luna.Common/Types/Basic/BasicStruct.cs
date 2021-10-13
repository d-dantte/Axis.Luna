using Axis.Luna.Common.Utils;
using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axis.Luna.Common.Types.Basic
{
    using Property = KeyValuePair<BasicStruct.PropertyName, BasicValue>;

    /// <summary>
    /// Represents a mutable mapping of names to basic values.
    /// This struct differs from all other BasicValues so far in that it's default state represents an EMPTY struct, rather than
    /// a null value.
    /// <para>
    ///   Significant implications of the above are:
    ///   <list type="number">
    ///     <item>
    ///       This struct is superficially immutable - meaning it's direct properties
    ///       are never changed, but the value those properties contain may be changed.
    ///     </item>
    ///     <item>
    ///       This struct's <c>default</c> value is an empty struct, not a null struct. As such, even the default value may mutate
    ///       into a non default value, and vice versa.
    ///     </item>
    ///     <item>
    ///       Internally, <c>default</c> of <see cref="BasicStruct"/>, at creation site, and before any mutation, holds null values for it's properties.
    ///       Making copies before mutation means that each copy will eventually be isolated from the original and the other copies when they are interacted with. 
    ///     </item>
    ///   </list>
    /// </para>
    /// </summary>
    public struct BasicStruct : IBasicValue<IEnumerable<Property>>
    {
        private BasicMetadata[] _metadata;
        private Dictionary<PropertyName, BasicValue> _properties;
        private Dictionary<string, PropertyName> _propertyBasicMetadata;
        private MapAccessor<string, BasicMetadata[]> _propertyBasicMetadataAccessor;

        #region Constructors
        public BasicStruct(IEnumerable<BasicMetadata> metadata)
            : this(metadata?.ToArray())
        {
        }

        public BasicStruct(params BasicMetadata[] metadata)
        {
            _metadata = null;
            _properties = null;
            _propertyBasicMetadata = null;
            _propertyBasicMetadataAccessor = default;

            __construct(metadata);
        }

        private void __construct(BasicMetadata[] metadata = null)
        {
            _properties = new Dictionary<PropertyName, BasicValue>();

            var propertyBasicMetadataClosure = _propertyBasicMetadata = new Dictionary<string, PropertyName>();

            _metadata = metadata?.Length > 0 == true
                ? metadata.ToArray()
                : null;
            _propertyBasicMetadataAccessor = new MapAccessor<string, BasicMetadata[]>(
                key => propertyBasicMetadataClosure.TryGetValue(key, out var name) ? name.Metadata : null);
        }
        #endregion

        #region Accessors
        public bool IsDefault => _properties == null;

        public bool HasProperty(string propertyName) => _properties?.ContainsKey(propertyName) == true;

        public BasicTypes Type => BasicTypes.Struct;

        public BasicMetadata[] Metadata => _metadata ?? Array.Empty<BasicMetadata>();

        public IEnumerable<Property> Value => _properties ?? (IEnumerable<Property>)Array.Empty<Property>();

        public PropertyName[] PropertyNames => _properties?.Keys.ToArray() ?? Array.Empty<PropertyName>();

        public IReadonlyIndexer<string, BasicMetadata[]> PropertyMetadata => _propertyBasicMetadataAccessor;

        public int Count => _properties?.Count ?? 0;

        public bool TryGetValue(PropertyName key, out BasicValue value)
        {
            if(_properties == null)
            {
                value = default;
                return false;
            }
            
            return _properties.TryGetValue(key, out value);
        }
        #endregion

        #region Mutators
        public bool Remove(PropertyName key) => PreMutate(@this => @this._properties.Remove(key));

        public BasicStruct Append(string key, BasicValue value) => PreMutate(@this =>
        {
            @this[key] = value;
            return @this;
        });

        public BasicStruct Append(PropertyName key, BasicValue value) => PreMutate(@this =>
        {
            @this[key] = value;
            return @this;
        });

        public BasicStruct Append(KeyValuePair<string, BasicValue> value) => Append(value.Key, value.Value);

        public BasicStruct Append(Property value) => Append(value.Key, value.Value);

        private R PreMutate<R>(Func<BasicStruct, R> mutator)
        {
            if (mutator == null)
                throw new ArgumentNullException(nameof(mutator));

            if (IsDefault)
                __construct();

            return mutator.Invoke(this); // <-- copy will work because the internal fields have been set
        }

        private void PreMutate(Action<BasicStruct> mutator)
        {
            if (mutator == null)
                throw new ArgumentNullException(nameof(mutator));

            if (IsDefault)
                __construct();

            mutator.Invoke(this); // <-- copy will work because the internal fields have been set
        }
        #endregion

        #region Indexers (Read/Write)
        /// <summary>
        /// Indexer for the property value identified by the supplied key.
        /// <para>
        /// Note that this indexer does not replace any metadata previously set for the target property.
        /// </para>
        /// </summary>
        /// <param name="key">the target property name</param>
        /// <returns>The property value, or an exception if the property is not present</returns>
        public BasicValue? this[string key]
        {
            get
            {
                if (TryGetValue(key, out var result))
                    return result;

                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException("Invalid Key");

                PreMutate(@this =>
                {
                    if (value == null)
                    {
                        @this._properties.Remove(key);
                        @this._propertyBasicMetadata.Remove(key);
                    }
                    else
                    {
                        @this._properties[key] = value.Value;
                    }
                });
            }
        }

        /// <summary>
        /// Indexer for the property value identified by the supplied key.
        /// <para>
        /// Note that this indexer replaces the metadata (deleting or replacing) with the value supplied by the <see cref="PropertyName"/>
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BasicValue? this[PropertyName key]
        {
            get
            {
                if (TryGetValue(key, out var result))
                    return result;

                return null;
            }
            set
            {
                if (key.IsDefault || string.IsNullOrEmpty(key.Name))
                    throw new ArgumentException("Invalid Key");

                PreMutate(@this =>
                {
                    @this._properties.Remove(key);
                    @this._propertyBasicMetadata.Remove(key.Name);

                    if (value != null)
                    {
                        @this._properties[key] = value.Value;
                        @this._propertyBasicMetadata[key.Name] = key;
                    }
                });
            }
        }
        #endregion

        #region Equality and Hashcodes
        private bool ContainsAllPropertiesOf(BasicStruct other)
        {
            var @this = this;
            return _properties?.All(kvp =>
            {
                if (!other.TryGetValue(kvp.Key, out var value))
                    return false;

                else
                    return kvp.Value.Equals(value);
            })
            ?? false;
        }

        public bool EquivalentTo(BasicStruct other)
        {
            if (this.IsDefault && other.IsDefault)
                return true;

            if (Count != other.Count)
                return false;

            if (!ContainsAllPropertiesOf(other))
                return false;

            else return true;
        }

        public bool ExactyCopyOf(BasicStruct other)
        {
            return other != null
                && this._metadata == other._metadata
                && this._properties == other._properties
                && this._propertyBasicMetadata == other._propertyBasicMetadata;
        }

        public override bool Equals(object obj)
        {
            return obj is BasicStruct other
                && (ExactyCopyOf(other) || EquivalentTo(other));
        }

        public override int GetHashCode()
        {
            var keyHash = Luna.Extensions.Common.ValueHash(
                _properties?.Keys.ToArray());

            var valueHash = Luna.Extensions.Common.ValueHash(
                _properties?.Values.ToArray());

            return Luna.Extensions.Common.ValueHash(keyHash, valueHash);
        }

        public static bool operator ==(BasicStruct first, BasicStruct second)
        {
            return first.Equals(second) == true;
        }

        public static bool operator !=(BasicStruct first, BasicStruct second) => !(first == second);
        #endregion


        /// <summary>
        /// Defines a property identifier for struct properties
        /// </summary>
        public struct PropertyName
        {
            private readonly Dictionary<string, string> _metadata;

            /// <summary>
            /// The name of the property
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// BasicMetadata possibly attached to the property (not to the property's value)
            /// </summary>
            public BasicMetadata[] Metadata => _metadata?.Select(kvp => new BasicMetadata(kvp)).ToArray() ?? Array.Empty<BasicMetadata>();

            public bool HasBasicMetadata => _metadata != null;

            /// <summary>
            /// Indicates if this value is the default value of this type.
            /// </summary>
            public bool IsDefault => Name == null;

            #region Constructors
            /// <summary>
            /// Constructs a new PropertyName. Note that the <c>Name</c> argument is not processed/parsed - it is assigned as-is.
            /// </summary>
            /// <param name="name">The name of the property</param>
            /// <param name="metadata">BasicMetadata information</param>
            public PropertyName(string name, params BasicMetadata[] metadata)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));

                _metadata = metadata?.Length > 0 == true
                    ? metadata?.Select(m => m.Key.ValuePair(m.Value)).ToDictionary()
                    : null;
            }

            /// <summary>
            /// Constructs a new PropertyName. Note that the <c>Name</c> argument is not processed/parsed - it is assigned as-is.
            /// </summary>
            /// <param name="name">The name of the property</param>
            /// <param name="metadata">BasicMetadata information</param>
            public PropertyName(string name, IEnumerable<BasicMetadata> metadata)
                :this(name, metadata?.ToArray())
            {
            }
            #endregion

            public bool BasicMetadataEquals(PropertyName other)
            {
                if (_metadata == null && other._metadata == null)
                    return true;

                else if (_metadata.Count == 0 && other._metadata.Count == 0)
                    return true;

                return _metadata?.ExactlyAll(kvp => other._metadata.TryGetValue(kvp.Key, out var v) && kvp.Value.NullOrEquals(v)) ?? false;
            }

            /// <summary>
            /// Only checks for name equality
            /// </summary>
            /// <param name="obj">The other object to test</param>
            /// <returns>indicates equality</returns>
            public override bool Equals(object obj)
            {
                return obj is PropertyName other
                    && other.Name.Equals(Name);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() => Name.GetHashCode();

            /// <summary>
            /// Returns this property name in its string format: <c>name[:: meta-key:meta-value; meta-key:meta-value; meta-value;]</c>
            /// <para>
            /// The implicit conversion from string honors this same exact format. Note that meta-key/value CANNOT accept ':' and ';' symbols in their names.
            /// </para>
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var metadata = _metadata?
                    .Select(kvp => kvp.Value != null ? $" {kvp.Key}:{kvp.Value};" : $"{kvp.Key};")
                    .JoinUsing("");

                metadata = metadata != null ? $"::{metadata}" : null;

                return $"{Name}{metadata}";
            }

            public static bool operator ==(PropertyName first, PropertyName second) => first.Equals(second);

            public static bool operator !=(PropertyName first, PropertyName second) => !first.Equals(second);

            /// <summary>
            /// See <see cref="BasicStruct.PropertyName.ToString"/> for an explanation of the string format accepted here.
            /// </summary>
            /// <param name="name"></param>
            public static implicit operator PropertyName(string name)
            {
                var parts = name?.Split("::", StringSplitOptions.None) ?? throw new ArgumentNullException(nameof(name));

                if (parts.Length == 1)
                    return new PropertyName(parts[0]);

                else if (parts.Length == 2)
                    return new PropertyName(parts[0].Trim(), Parse(parts[1].Trim()));

                else
                    throw new ArgumentException("Invalid name: " + name);
            }

            private static BasicMetadata[] Parse(string metadataString)
            {
                return metadataString
                    .Split(';')
                    .Select(ToKvp)
                    .ToArray();
            }

            private static BasicMetadata ToKvp(string kvpString)
            {
                var parts = kvpString.Split(':');

                if (parts.Length == 0 || parts.Length > 2)
                    throw new ArgumentException("Invalid string: " + kvpString);

                return new BasicMetadata(
                    key: parts[0].Trim(), 
                    value: parts.Length > 1 ? parts[1].Trim() : null);
            }
        }

        /// <summary>
        /// Readonly Indexer implementation for accessing the metadata array given a key
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private struct MapAccessor<TKey, TValue>: IReadonlyIndexer<TKey, TValue>
        {
            private readonly Func<TKey, TValue> _accessor;

            public bool IsDefault => _accessor == null;

            public TValue this[TKey key] => !IsDefault 
                ? _accessor.Invoke(key)
                : throw new InvalidOperationException($"indexing on default {nameof(MapAccessor<TKey, TValue>)} is forbidden.");

            public MapAccessor(Func<TKey, TValue> accessor)
            {
                _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            }

            public override bool Equals(object obj)
            {
                return obj is MapAccessor<TKey, TValue> other
                    && other._accessor == _accessor;
            }

            public override int GetHashCode() => HashCode.Combine(_accessor);

            public static bool operator ==(MapAccessor<TKey, TValue> first, MapAccessor<TKey, TValue> second) => first.Equals(second);
            public static bool operator !=(MapAccessor<TKey, TValue> first, MapAccessor<TKey, TValue> second) => !first.Equals(second);
        }
    }
}
