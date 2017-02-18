using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    public class DomainConverter
    {
        private static readonly string CallContextKey = "Axis.Luna.CallContext.Key";
        public readonly ConversionRegistry Converters = new ConversionRegistry();


        public void LoadConverters(Action<ConversionRegistry> conversionRegistrations)
        {
            conversionRegistrations?.Invoke(Converters);
        }               

        public To Convert<From, To>(From obj)
        {
            var isEntry = !ContextExists();
            try
            {
                var context = AcquireContext();

                if (context.IsCached(obj))
                    return (To)context.GetCachedValue(obj);
                else
                {
                    var operations = FindConversionOperation(new ConversionVector { From = typeof(From), To = typeof(To) });

                    //generate the value
                    var value = operations.Generator == null ?
                                Activator.CreateInstance<To>() :
                                ((Func<From, To>)operations.Generator).Invoke(obj);

                    //cache the value
                    context.CacheValue(obj, value);

                    //map the value
                    ((Action<From, To>)operations.Converter).Invoke(obj, value);

                    return value;
                }
            }
            finally
            {
                if (isEntry) DestroyContext();
            }
        }

        private bool ContextExists() => CallContext.LogicalGetData(CallContextKey) != null;

        private ConversionContext AcquireContext()
        {
            var context = CallContext.LogicalGetData(CallContextKey) as ConversionContext;
            if (context == null) CallContext.LogicalSetData(CallContextKey, context = new ConversionContext());

            return context;
        }

        private void DestroyContext() => CallContext.FreeNamedDataSlot(CallContextKey);

        private ConversionOperations FindConversionOperation(ConversionVector vector)
        {
            if (Converters.TypeRegistry.ContainsKey(vector)) return Converters.TypeRegistry[vector];

            try
            {
                //find in the registry, the vector who's "From" type is the closest ancestor to the arguments vector's "From" type
                var ancestors = vector.From.BaseTypes().ToList();
                var childVector = Converters.TypeRegistry.Keys
                    .Select(_vector => new { Delta = ancestors.IndexOf(_vector.From), Vector = _vector })
                    .Where(__vector => __vector.Delta >= 0)
                    .Where(__vector => __vector.Vector.To == vector.To)
                    .Aggregate((_current, _next) => _next.Delta < _current.Delta ? _next : _current); //will throw an exception if nothing is found, which is fine

                return Converters.TypeRegistry[childVector.Vector];
            }
            catch (Exception e)
            {
                throw new Exception("Conversion not found", e);
            }
        }


        public class ConversionRegistry
        {
            internal readonly Dictionary<ConversionVector, ConversionOperations> TypeRegistry = new Dictionary<ConversionVector, ConversionOperations>();

            public int Count => TypeRegistry.Count;

            public ConversionRegistry Add<From, To>(Action<From, To, DomainConverter> conversion) => Add(null, conversion);
            public ConversionRegistry Add<From, To>(Func<From, To> generator, Action<From, To, DomainConverter> conversion)
            {
                var etype = typeof(To);
                var mtype = typeof(From);

                if (conversion == null)
                    throw new Exception("invalid conversion");

                TypeRegistry.Add(new ConversionVector { From = mtype, To = etype }, new ConversionOperations { Generator = generator, Converter = conversion });

                return this;
            }

            internal ConversionRegistry()
            { }
        }

        internal class ConversionVector
        {
            internal Type From { get; set; }
            internal Type To { get; set; }

            public override int GetHashCode() => ValueHash(new object[] { ThrowIfNull(From), ThrowIfNull(To) });
            public override bool Equals(object obj)
            {
                var other = (ConversionVector)obj;
                return other != null &&
                       other.From == From &&
                       other.To == To &&
                       other.GetHashCode() == GetHashCode();
            }

            public Type ThrowIfNull(Type t)
            {
                if (t == null) throw new NullReferenceException();
                else return t;
            }
        }

        internal class ConversionOperations
        {
            internal Delegate Generator { get; set; }
            internal Delegate Converter { get; set; }
        }
    }

    public class ConversionContext
    {
        private Dictionary<object, object> _cache = new Dictionary<object, object>();


        public bool IsCached(object from) => _cache.ContainsKey(from);

        internal ConversionContext CacheValue(object from, object to)
        {
            _cache[from] = to;
            return this;
        }

        internal object GetCachedValue(object from) => _cache[from];
    }
}
