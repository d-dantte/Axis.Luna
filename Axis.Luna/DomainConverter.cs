using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{

    public class DomainConverter
    {
        private Registry _registry = new Registry();

        public DomainConverter(Action<Registry> conversionRegistration)
        {
            conversionRegistration.Invoke(_registry);
        }

        public Y Convert<X, Y>(X obj, ConversionContext context = null)
            => (Y)(context ?? (context = new ConversionContext(this))).GetOrAdd(obj, _obj =>
            {
                var _dynamic = _registry.TypeRegistry[new ConversionVector { From = typeof(X), To = typeof(Y) }];
                var converter = (Func<X, ConversionContext, Y>)_dynamic;
                return converter.Invoke(obj, context);
            });


        /// <summary>
        /// Converts using the first conversion function it finds with <c>X</c> as the source-type, returning the result as an object
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Convert<X>(X obj, ConversionContext context = null)
            => (context ?? (context = new ConversionContext(this))).GetOrAdd(obj, _obj =>
            {
                var tx = typeof(X);
                var _dynamic = _registry.TypeRegistry.First(_kvp => _kvp.Key.From == tx).Value;
                return _dynamic.Invoke(obj, context);
            });

        /// <summary>
        /// Converts using the first conversion function it finds with <c>X</c> as the source-type, returning the result as a <c>dynamic</c>
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public dynamic ConvertDynamic<X>(X obj, ConversionContext context = null)
            => (context ?? (context = new ConversionContext(this))).GetOrAdd(obj, _obj =>
            {
                var tx = typeof(X);
                var _dynamic = _registry.TypeRegistry.First(_kvp => _kvp.Key.From == tx).Value;
                return _dynamic.Invoke(obj, context);
            });




        public class Registry
        {
            internal readonly Dictionary<ConversionVector, dynamic> TypeRegistry = new Dictionary<ConversionVector, dynamic>();

            public Registry Register<Domain, Entity>(Func<Domain, ConversionContext, Entity> toEntity,
                                                     Func<Entity, ConversionContext, Domain> toDomain)
            {
                var etype = typeof(Entity);
                var dtype = typeof(Domain);

                if (toEntity == null ||
                    toDomain == null)
                    throw new Exception("invalid conversion");

                TypeRegistry.Add(new ConversionVector { From = etype, To = dtype }, toDomain);
                TypeRegistry.Add(new ConversionVector { From = dtype, To = etype }, toEntity);

                return this;
            }

            internal Registry()
            { }
        }

        internal class ConversionVector
        {
            internal Type From { get; set; }
            internal Type To { get; set; }

            public override int GetHashCode() => ValueHash(new object[] { From.ThrowIfNull(), To.ThrowIfNull() });
            public override bool Equals(object obj)
            {
                var other = obj.As<ConversionVector>();
                return other != null &&
                       other.From == From &&
                       other.To == To &&
                       other.GetHashCode() == GetHashCode();
            }
        }
    }

    public class ConversionContext
    {
        private Dictionary<object, object> _cache = new Dictionary<object, object>();

        public DomainConverter Converter { get; private set; }


        public bool IsCached(object from) => _cache.ContainsKey(from);

        internal ConversionContext CacheValue(object from, object to) => this.UsingValue(@this => _cache[from] = to);

        internal object GetCachedValue(object from) => _cache[from];

        internal object GetOrAdd(object from, Func<object, object> valueGenerator) => _cache.GetOrAdd(from, _from => valueGenerator(_from));

        internal ConversionContext(DomainConverter converter)
        {
            Converter = converter;
        }
    }
}
