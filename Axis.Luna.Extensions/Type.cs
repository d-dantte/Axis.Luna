using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Axis.Luna.Extensions
{
    //[DebuggerStepThrough]
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> TypeDefaultsProducer = new ConcurrentDictionary<Type, Func<object>>();
        private static readonly ConcurrentDictionary<Type, string> MinimalAQNames = new ConcurrentDictionary<Type, string>();

        private const string ExplicitOperatorName = "op_Explicit";
        private const string ImplicitOperatorName = "op_Implicit";


        #region Attributes

        public static bool HasAttribute(this Type type, Type attributeType)
        => type.GetCustomAttribute(attributeType) != null;

        public static bool HasAttribute<A>(this Type type)
        where A : Attribute => type.GetCustomAttribute<A>() != null;

        public static bool HasAttribute(this MemberInfo member, Type attributeType)
        => member.GetCustomAttribute(attributeType) != null;

        public static bool HasAttribute<A>(this MemberInfo member)
        where A : Attribute => member.GetCustomAttribute<A>() != null;

        #endregion

        #region Type Names and Signatures
        public static string MinimalAQName(this
            Type type)
            => MinimalAQNames.GetOrAdd(type, t =>
            {
                var sb = new StringBuilder($"{t.Namespace}.{t.Name}");
                if (t.IsGenericType && !t.IsGenericTypeDefinition)
                    sb.Append('[')
                      .Append(t.GetGenericArguments()
                               .Aggregate("", (ssb, n) => ssb += $"{(ssb.Length > 0 ? ", " : "")}[{n.MinimalAQName()}]"))
                      .Append(']');

                return sb.Append($", {t.Assembly.GetName().Name}").ToString();
            });

        public static string MinimalAQSignature(this Delegate d) => d.Method.MinimalAQSignature();

        public static string MinimalAQSignature(this MethodInfo m)
        {
            var builder = new StringBuilder();
            builder
                .Append('[')
                .Append(m.DeclaringType.MinimalAQName())
                .Append(']')
                .Append('.')
                .Append(m.Name)
                .Append(!m.IsGenericMethod ? "" :
                        "<" + m.GetGenericArguments().Aggregate("", (@params, param) => @params += (@params == "" ? "" : ", ") + "[" + param.MinimalAQName() + "]") + ">")
                .Append('(')
                .Append(m.GetParameters()
                         .Aggregate("", (@params, param) => @params += (@params == "" ? "" : ", ") + "[" + param.ParameterType.MinimalAQName() + "] "))
                .Append(')')
                .Append("::")
                .Append('[').Append(m.ReturnType.MinimalAQName()).Append("]");

            return builder.ToString();
        }

        private static string AccessorSignature(this PropertyInfo pinfo)
        => $"[{pinfo.DeclaringType.MinimalAQName()}].Get{pinfo.Name}";

        private static string MutatorSignature(this PropertyInfo pinfo)
        => $"[{pinfo.DeclaringType.MinimalAQName()}].Set{pinfo.Name}";
        #endregion

        #region Inheritance

        /// <summary>
        /// Verifies that the given GENERIC interface type has the supplied generic type definition defined on it.
        /// </summary>
        /// <param name="interfaceType">The generic interface</param>
        /// <param name="genericTypeDefinition">The generic interface type definition</param>
        public static bool HasGenericInterfaceDefinition(this Type interfaceType, Type genericTypeDefinition)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (!interfaceType.IsInterface || !interfaceType.IsGenericType)
                return false;

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                return false;

            return interfaceType.GetGenericTypeDefinition().Equals(genericTypeDefinition);
        }

        /// <summary>
        /// Verifies that the given GENERIC type has the supplied generic type definition defined on it.
        /// </summary>
        /// <param name="genericType">The generic interface</param>
        /// <param name="genericTypeDefinition">The generic interface type definition</param>
        public static bool HasGenericTypeDefinition(this Type genericType, Type genericTypeDefinition)
        {
            if (genericType == null)
                throw new ArgumentNullException(nameof(genericType));

            if (genericTypeDefinition == null)
                throw new ArgumentNullException(nameof(genericTypeDefinition));

            if (genericType.IsInterface || !genericType.IsGenericType)
                return false;

            if (!genericTypeDefinition.IsGenericTypeDefinition)
                return false;

            return genericType.GetGenericTypeDefinition().Equals(genericTypeDefinition);
        }

        /// <summary>
        /// Verifies that the given <paramref name="type"/> has an generic ancestor with <paramref name="genericDefinitionBaseType"/>
        /// as its generic type definition.
        /// <para/>
        /// NOTE: this will fail if <paramref name="genericDefinitionBaseType"/> is the <c>GenericTypeDefinition</c> of <c>type</c>
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericDefinitionBaseType">The generic type definition to check the ancestors against</param>
        public static bool HasBaseGenericType(this Type type, Type genericDefinitionBaseType)
        {
            if (!genericDefinitionBaseType.IsGenericTypeDefinition)
                throw new ArgumentException($"Invalid {nameof(genericDefinitionBaseType)}: ancestor is not a generic type definition");

            EqualityComparer<Type> comparer = EqualityComparer<Type>.Default;
            return type
                .BaseTypes()
                .Where(_bt => _bt != type)
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => comparer.Equals(_bt.GetGenericTypeDefinition(), genericDefinitionBaseType))
                .Any();
        }

        /// <summary>
        /// Verifies that one of the interfaces implemented by <paramref name="type"/> has
        /// <paramref name="genericDefinitionInterfaceType"/> as it's generic type definition.
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericDefinitionInterfaceType">The generic type definition to check interfaces against</param>
        public static bool ImplementsGenericInterface(this
            Type type,
            Type genericDefinitionInterfaceType)
            => type
                .GetInterfaces()
                .Any(_i => _i.IsGenericType && _i.GetGenericTypeDefinition() == genericDefinitionInterfaceType);


        /// <summary>
        /// Gets the base type who has the given <paramref name="genericBaseDefinition"/> as its generic type definition.
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericBaseDefinition">The type definition to check base types against</param>
        public static Type GetGenericBase(this Type type, Type genericBaseDefinition)
        {
            genericBaseDefinition
                .ThrowIf(
                    t => !t.IsGenericTypeDefinition,
                    _ => new ArgumentException("base type is not a generic type definition"))
                .ThrowIf(
                    t => !t.IsClass,
                    _ => new ArgumentException($"supplied {nameof(genericBaseDefinition)} type is not a class"));

            return type
                .BaseTypes()
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericBaseDefinition)
                .FirstOrDefault();
        }

        /// <summary>
        /// Similar to  <see cref="GetGenericBase(Type, Type)"/>, except using the <c>TryXXX</c> pattern: returns true
        /// if the base is found, false otherwise, in addition to populating the <c>out</c> parameter when found.
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericBaseDefinition">The generic type definition to check base types against</param>
        /// <param name="genericBase">The base type if found</param>
        /// <returns>True if the base type is found, false otherwise</returns>
        public static bool TryGetGenericBase(this Type type, Type genericBaseDefinition, out Type genericBase)
        {
            genericBase = null;

            if (!genericBaseDefinition.IsGenericTypeDefinition)
                return false;

            if (!genericBaseDefinition.IsClass)
                return false;

            genericBase = type
                .BaseTypes()
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericBaseDefinition)
                .FirstOrDefault();

            return genericBase != null;
        }

        /// <summary>
        /// Gets the interface who has the given <paramref name="genericBaseDefinition"/> as its generic type definition.
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericDefinitionInterface">The generic type definition to check interfaces against</param>
        public static Type GetGenericInterface(this Type type, Type genericDefinitionInterface)
        {
            genericDefinitionInterface
                .ThrowIf(
                    t => !t.IsGenericTypeDefinition,
                    _ => new ArgumentException("interface is not a generic type definition"))
                .ThrowIf(
                    t => !t.IsInterface,
                    _ => new ArgumentException($"supplied {nameof(genericDefinitionInterface)} type is not an interface"));

            return type
                .GetInterfaces()
                .Where(_i => _i.IsGenericType)
                .Where(_i => _i.GetGenericTypeDefinition() == genericDefinitionInterface)
                .FirstOrDefault();
        }

        /// <summary>
        /// Similar to  <see cref="GetGenericInterface(Type, Type)(Type, Type)"/>, except using the <c>TryXXX</c> pattern: returns true
        /// if the interface is found, false otherwise, in addition to populating the <c>out</c> parameter when found.
        /// </summary>
        /// <param name="type">The type to verify</param>
        /// <param name="genericInterfaceDefinition">The generic type definition to check interfaces against</param>
        /// <param name="genericInterface">The interface, if found</param>
        /// <returns>True if the interface is found, false otherwise</returns>
        public static bool TryGetGenericInterface(this Type type, Type genericInterfaceDefinition, out Type genericInterface)
        {
            genericInterface = null;

            if (!genericInterfaceDefinition.IsGenericTypeDefinition)
                return false;

            if (!genericInterfaceDefinition.IsInterface)
                return false;

            genericInterface = type
                .GetInterfaces()
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericInterfaceDefinition)
                .FirstOrDefault();

            return genericInterface != null;
        }

        /// <summary>
        /// Verifies that the given type implements all of the supplied interfaces.
        /// </summary>
        /// <param name="type">The type to test against. Can be a class or struct, or interface, etc.</param>
        /// <param name="interfaces">Multiple interfaces to check for implementation</param>
        public static bool Implements(this Type type, params Type[] interfaces)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(interfaces);

            if (interfaces.IsEmpty())
                return true;

            var interfaceSet = new HashSet<Type>(type.GetInterfaces());
            return interfaces
                .ThrowIfAny(
                    i => !i.IsInterface,
                    i => new ArgumentException($"{i} is not an interface"))
                .All(interfaceSet.Contains);
        }

        /// <summary>
        /// Verifies that the given type implements any of the supplied interfaces.
        /// </summary>
        /// <param name="type">The type to test against. Can be a class or struct, or interface, etc.</param>
        /// <param name="interfaces">Multiple interfaces to check for implementation</param>
        public static bool ImplementsAny(this Type type, params Type[] interfaces)
        {
            var interfaceSet = new HashSet<Type>(type.GetInterfaces());
            return interfaces
                .ThrowIfAny(
                    i => !i.IsInterface,
                    i => new ArgumentException($"{i} is not an interface"))
                .Any(interfaceSet.Contains);
        }

        /// <summary>
        /// Ensures that a type inherits from all the listed types. If any of the bases is a GenericTypeDefinition, the method checks that any of the
        /// target type's generic bases has a generic definition that matches the given one.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bases"></param>
        public static bool Extends(this Type type, params Type[] bases)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (bases is null)
                return false;

            if (bases.IsEmpty())
                return true;

            var regularBases = new HashSet<Type>();
            var genericDefinitionBases = new HashSet<Type>();

            bases.ForEvery(@base =>
            {
                if (!@base.IsClass)
                    throw new InvalidOperationException($"{@base} is not a class");

                if (@base.IsGenericTypeDefinition)
                    genericDefinitionBases.Add(@base);

                else regularBases.Add(@base);
            });

            var actualBases = type
                .BaseTypes()
                .SelectMany(FlattenIfGeneric)
                .ApplyTo(types => new HashSet<Type>(types));

            //check bases are contained. Note that empty collections return true for the "All(...)" function.
            return regularBases.All(@base => actualBases.Contains(@base))
                && genericDefinitionBases.All(@base => actualBases.Contains(@base));
        }

        /// <summary>
        /// Gets all of the types and interfaces in a types inheritance lineage.
        /// </summary>
        /// <param name="type">The type to query</param>
        /// <returns>All types found in its lineage</returns>
        public static IEnumerable<Type> TypeLineage(this Type type) => type.GetInterfaces().Concat(type.BaseTypes());

        /// <summary>
        /// Returns this type, along with all the types it inherits from, all the way back to "object"
        /// </summary>
        /// <param name="type">The type to query</param>
        /// <returns></returns>
        public static IEnumerable<Type> BaseTypes(this Type type)
        {
            Type @base = type;
            while (@base != null)
            {
                yield return @base;
                @base = @base.BaseType;
            }
        }


        public static bool IsOrImplementsGenericInterface(this Type targetType, Type genericDefinitionInterface)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            if (genericDefinitionInterface is null)
                throw new ArgumentNullException(nameof(genericDefinitionInterface));

            if (!genericDefinitionInterface.IsInterface)
                throw new ArgumentException($"Interface '{genericDefinitionInterface}' must be an interface");

            if (!genericDefinitionInterface.IsGenericTypeDefinition)
                throw new ArgumentException($"Interfce '{genericDefinitionInterface}' must be a generic type definition");

            if (targetType.HasGenericInterfaceDefinition(genericDefinitionInterface))
                return true;

            else return targetType.ImplementsGenericInterface(genericDefinitionInterface);
        }

        public static bool IsOrExtendsGenericBase(this Type targetType, Type genericDefinitionBase)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            if (genericDefinitionBase is null)
                throw new ArgumentNullException(nameof(genericDefinitionBase));

            if (genericDefinitionBase.IsInterface)
                throw new ArgumentException($"base type '{genericDefinitionBase}' must not be an interface");

            return targetType
                .TypeLineage()
                .Where(type => type.IsGenericType)
                .Select(type => type.GetGenericTypeDefinition())
                .Any(definition => definition.Equals(genericDefinitionBase));
        }

        #endregion

        #region Custom Converters
        public static MethodInfo ExplicitConverterTo<TOut>(this Type sourceType)
        {
            return !sourceType.TryGetExplicitConverterTo<TOut>(out var method)
                ? throw new MissingMethodException("No explicit converter found for the type")
                : method;
        }

        public static MethodInfo ExplicitConverterTo(this Type sourceType, Type destinationType)
        {
            return !sourceType.TryGetExplicitConverterTo(destinationType, out var method)
                ? throw new MissingMethodException("No explicit converter found for the type")
                : method;
        }

        public static MethodInfo ImplicitConverterTo<TOut>(this Type sourceType)
        {
            return !sourceType.TryGetImplicitConverterTo<TOut>(out var method)
                ? throw new MissingMethodException("No implicit converter found for the type")
                : method;
        }

        public static MethodInfo ImplicitConverterTo(this Type sourceType, Type destinationType)
        {
            return !sourceType.TryGetImplicitConverterTo(destinationType, out var method)
                ? throw new MissingMethodException("No implicit converter found for the type")
                : method;
        }

        public static MethodInfo ExplicitConverterFrom<TIn>(this Type destinationType)
        {
            return !destinationType.TryGetExplicitConverterFrom<TIn>(out var method)
                ? throw new MissingMethodException("No explicit converter found for the type")
                : method;
        }

        public static MethodInfo ExplicitConverterFrom(this Type destinationType, Type sourceType)
        {
            return !destinationType.TryGetExplicitConverterFrom(sourceType, out var method)
                ? throw new MissingMethodException("No explicit converter found for the type")
                : method;
        }

        public static MethodInfo ImplicitConverterFrom<TIn>(this Type destinationType)
        {
            return !destinationType.TryGetImplicitConverterFrom<TIn>(out var method)
                ? throw new MissingMethodException("No implicit converter found for the type")
                : method;
        }

        public static MethodInfo ImplicitConverterFrom(this Type destinationType, Type sourceType)
        {
            return !destinationType.TryGetImplicitConverterFrom(sourceType, out var method)
                ? throw new MissingMethodException("No implicit converter found for the type")
                : method;
        }

        public static bool TryGetExplicitConverterTo<TOut>(this Type sourceType, out MethodInfo converter)
            => sourceType.TryGetExplicitConverterTo(typeof(TOut), out converter);

        public static bool TryGetExplicitConverterTo(this Type sourceType, Type destinationType, out MethodInfo converter)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            converter = sourceType
                .GetMethods()
                .Where(minfo => ExplicitOperatorName.Equals(minfo.Name))
                .Where(minfo => minfo.IsStatic)
                .Where(minfo => minfo.IsSpecialName)
                .Where(minfo =>
                {
                    var @params = minfo.GetParameters();
                    return @params.Length == 1
                        && @params[0].ParameterType.Equals(sourceType);
                })
                .Where(minfo => minfo.ReturnType.Equals(destinationType))
                .FirstOrDefault();

            return converter != null;
        }

        public static bool TryGetImplicitConverterTo<TOut>(this Type sourceType, out MethodInfo converter)
            => sourceType.TryGetImplicitConverterTo(typeof(TOut), out converter);

        public static bool TryGetImplicitConverterTo(this Type sourceType, Type destinationType, out MethodInfo converter)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            converter = sourceType
                .GetMethods()
                .Where(minfo => ImplicitOperatorName.Equals(minfo.Name))
                .Where(minfo => minfo.IsStatic)
                .Where(minfo => minfo.IsSpecialName)
                .Where(minfo =>
                {
                    var @params = minfo.GetParameters();
                    return @params.Length == 1
                        && @params[0].ParameterType.Equals(sourceType);
                })
                .Where(minfo => minfo.ReturnType.Equals(destinationType))
                .FirstOrDefault();

            return converter != null;
        }

        public static bool TryGetExplicitConverterFrom<TIn>(this Type destinationType, out MethodInfo converter)
            => destinationType.TryGetExplicitConverterFrom(typeof(TIn), out converter);

        public static bool TryGetExplicitConverterFrom(this Type destinationType, Type sourceType, out MethodInfo converter)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            converter = destinationType
                .GetMethods()
                .Where(minfo => ExplicitOperatorName.Equals(minfo.Name))
                .Where(minfo => minfo.IsStatic)
                .Where(minfo => minfo.IsSpecialName)
                .Where(minfo =>
                {
                    var @params = minfo.GetParameters();
                    return @params.Length == 1
                        && @params[0].ParameterType.Equals(sourceType);
                })
                .Where(minfo => minfo.ReturnType.Equals(destinationType))
                .FirstOrDefault();

            return converter != null;
        }

        public static bool TryGetImplicitConverterFrom<TIn>(this Type destinationType, out MethodInfo converter)
            => destinationType.TryGetImplicitConverterFrom(typeof(TIn), out converter);

        public static bool TryGetImplicitConverterFrom(this Type destinationType, Type sourceType, out MethodInfo converter)
        {
            if (destinationType is null)
                throw new ArgumentNullException(nameof(destinationType));

            converter = destinationType
                .GetMethods()
                .Where(minfo => ImplicitOperatorName.Equals(minfo.Name))
                .Where(minfo => minfo.IsStatic)
                .Where(minfo => minfo.IsSpecialName)
                .Where(minfo =>
                {
                    var @params = minfo.GetParameters();
                    return @params.Length == 1
                        && @params[0].ParameterType.Equals(sourceType);
                })
                .Where(minfo => minfo.ReturnType.Equals(destinationType))
                .FirstOrDefault();

            return converter != null;
        }
        #endregion

        #region Misc

        public static object DefaultValue(this Type type)
        {
            if (type.IsValueType)
            {
                var producer = TypeDefaultsProducer.GetOrAdd(type, _t =>
                {
                    var @default = Expression.Default(_t);
                    var cast = Expression.Convert(@default, typeof(object));
                    var lambda = Expression.Lambda(typeof(Func<object>), cast);
                    return lambda.Compile().As<Func<object>>();
                });

                return producer.Invoke();
            }
            else return null;
        }

        public static bool IsPropertyAccessor(this MethodInfo method)
        => method.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == method);

        public static bool IsPropertyMutator(this MethodInfo method)
        => method.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == method);

        public static bool IsIntegral(this Type type) => Integrals.Contains(type);

        public static bool IsDecimal(this Type type) => typeof(decimal).Equals(type);

        public static bool IsReal(this Type type) => Reals.Contains(type);

        internal static readonly IEnumerable<Type> Integrals = new HashSet<Type>()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };

        internal static readonly IEnumerable<Type> Reals = new HashSet<Type>()
        {
            typeof(Half),
            typeof(float),
            typeof(double)
        };

        #endregion

        private static IEnumerable<Type> FlattenIfGeneric(Type type)
        {
            yield return type;

            if (type.IsGenericType)
                yield return type.GetGenericTypeDefinition();
        }
    }
}
