using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Axis.Luna.FInvoke
{
    public class TypeInvoker
    {
        private readonly Dictionary<MethodInfo, StaticInvoker> staticMethods;

        private readonly Dictionary<string, StaticInvoker> staticPropertySetters;

        private readonly Dictionary<string, StaticInvoker> staticPropertyGetters;

        private readonly Dictionary<MethodInfo, InstanceInvoker> instanceMethods;

        private readonly Dictionary<string, InstanceInvoker> instancePropertySetters;

        private readonly Dictionary<string, InstanceInvoker> instancePropertyGetters;

        private readonly Dictionary<ConstructorInfo, ConstructorInvoker> constructors;

        #region Default flags
        public const BindingFlags DefaultInstanceMethodSelector =
            BindingFlags.Public
            | BindingFlags.Instance;

        public const BindingFlags DefaultInstancePropertySelector =
            BindingFlags.Public
            | BindingFlags.Instance;

        public const BindingFlags DefaultStaticMethodSelector =
            BindingFlags.Public
            | BindingFlags.Static;

        public const BindingFlags DefaultStaticPropertySelector =
            BindingFlags.Public
            | BindingFlags.Static;

        public const BindingFlags DefaultConstructorSelector =
            BindingFlags.Public
            | BindingFlags.Instance;
        #endregion

        #region Properties

        /// <summary>
        /// The type from which the invokers are built.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<ConstructorInfo, ConstructorInvoker> ConstructorInvokers => new(constructors);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<MethodInfo, InstanceInvoker> InstanceMethodInvokers => new(instanceMethods);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<string, InstanceInvoker> InstanceSetters => new(instancePropertySetters);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<string, InstanceInvoker> InstanceGetters => new(instancePropertyGetters);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<MethodInfo, StaticInvoker> StaticMethodInvokers => new(staticMethods);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<string, StaticInvoker> StaticSetters => new(staticPropertySetters);

        /// <summary>
        /// 
        /// </summary>
        public ReadonlyInvokerIndexer<string, StaticInvoker> StaticGetters => new(staticPropertyGetters);
        #endregion

        public TypeInvoker(
            Type type,
            BindingFlags instanceMethodSelector = DefaultInstanceMethodSelector,
            BindingFlags instancePropertySelector = DefaultInstancePropertySelector,
            BindingFlags staticMethodSelector = DefaultStaticMethodSelector,
            BindingFlags staticPropertySelector = DefaultStaticPropertySelector,
            BindingFlags constructorSelector = DefaultConstructorSelector)
        {
            ArgumentNullException.ThrowIfNull(type);

            Type = type;

            // constructor invokers
            constructors = type
                .GetConstructors(constructorSelector)
                .Select(ctor => (Constructor: ctor, Invoker: ConstructorInvoker.InvokerFor(ctor)))
                .ToDictionary(map => map.Constructor, map => map.Invoker);

            #region Instance
            // instance property setters invokers
            instancePropertySetters = type
                .GetProperties(instancePropertySelector)
                .Where(property => property.CanWrite)
                .Select(property => (property.Name, Invoker: InstanceInvoker.InvokerFor(property.GetSetMethod())))
                .ToDictionary(map => map.Name, map => map.Invoker);

            // instance property getters invokers
            instancePropertyGetters = type
                .GetProperties(instancePropertySelector)
                .Where(property => property.CanRead)
                .Select(property => (property.Name, Invoker: InstanceInvoker.InvokerFor(property.GetGetMethod())))
                .ToDictionary(map => map.Name, map => map.Invoker);

            var propertyMethods = type
                .GetProperties(instancePropertySelector)
                .SelectMany(property => new[] { property.GetGetMethod(), property.GetSetMethod() })
                .Where(method => method is not null)
                .ApplyTo(methods => new HashSet<MethodInfo>(methods));

            // instance method invokers
            instanceMethods = type
                .GetMethods(instanceMethodSelector)
                .Where(method => !propertyMethods.Contains(method))
                .Select(method => (Method: method, Invoker: InstanceInvoker.InvokerFor(method)))
                .ToDictionary(map => map.Method, map => map.Invoker);
            #endregion

            #region Static
            // instance property setters invokers
            staticPropertySetters = type
                .GetProperties(staticPropertySelector)
                .Where(property => property.CanWrite)
                .Select(property => (property.Name, Invoker: StaticInvoker.InvokerFor(property.GetSetMethod())))
                .ToDictionary(map => map.Name, map => map.Invoker);

            // instance property getters invokers
            staticPropertyGetters = type
                .GetProperties(staticPropertySelector)
                .Where(property => property.CanRead)
                .Select(property => (property.Name, Invoker: StaticInvoker.InvokerFor(property.GetGetMethod())))
                .ToDictionary(map => map.Name, map => map.Invoker);

            propertyMethods = type
                .GetProperties(staticPropertySelector)
                .SelectMany(property => new[] { property.GetGetMethod(), property.GetSetMethod() })
                .Where(method => method is not null)
                .ApplyTo(methods => new HashSet<MethodInfo>(methods));

            // instance method invokers
            staticMethods = type
                .GetMethods(staticMethodSelector)
                .Where(method => !propertyMethods.Contains(method))
                .Select(method => (Method: method, Invoker: StaticInvoker.InvokerFor(method)))
                .ToDictionary(map => map.Method, map => map.Invoker);
            #endregion
        }

        public static TypeInvoker Of(
            Type type,
            BindingFlags instanceMethodSelector = DefaultInstanceMethodSelector,
            BindingFlags instancePropertySelector = DefaultInstancePropertySelector,
            BindingFlags staticMethodSelector = DefaultStaticMethodSelector,
            BindingFlags staticPropertySelector = DefaultStaticPropertySelector,
            BindingFlags constructorSelector = DefaultConstructorSelector)
            => new(
                type,
                instanceMethodSelector,
                instancePropertySelector,
                staticMethodSelector,
                staticPropertySelector,
                constructorSelector);


        #region Nested types
        public readonly struct ReadonlyInvokerIndexer<TKey, TValue>
        {
            private readonly IDictionary<TKey, TValue> map;

            public ReadonlyInvokerIndexer(IDictionary<TKey, TValue> map)
            {
                ArgumentNullException.ThrowIfNull(map);
                this.map = map;
            }

            public int Count => map.Count;

            public bool IsMapped(TKey key) => map.ContainsKey(key);

            public TValue this[TKey key]
            {
                get => map[key];
            }

            public bool TryGetValue(TKey method, out TValue result)
            {
                return map.TryGetValue(method, out result);
            }

            public IEnumerable<TKey> Keys => map.Keys;

            public IEnumerable<TValue> Invokers => map.Values;
        }
        #endregion

    }
}
