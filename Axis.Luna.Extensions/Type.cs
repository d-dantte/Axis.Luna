using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Axis.Luna.Extensions
{

    //[DebuggerStepThrough]
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> TypeDefaultsProducer = new ConcurrentDictionary<Type, Func<object>>();
        private static readonly ConcurrentDictionary<Type, string> MinimalAQNames = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<string, Delegate> PropertyAccessors = new ConcurrentDictionary<string, Delegate>();

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
        public static string MinimalAQName(this Type type)
        => MinimalAQNames.GetOrAdd(type, t =>
        {
            var sb = new StringBuilder($"{t.Namespace}.{t.Name}");
            if (t.IsGenericType && !t.IsGenericTypeDefinition)
                sb.Append("[")
                  .Append(t.GetGenericArguments()
                           .Aggregate("", (ssb, n) => ssb += $"{(ssb.Length > 0 ? ", " : "")}[{n.MinimalAQName()}]"))
                  .Append("]");

            return sb.Append($", {t.Assembly.GetName().Name}").ToString();
        });

        public static string MinimalAQSignature(this Delegate d) => d.Method.MinimalAQSignature();

        public static string MinimalAQSignature(this MethodInfo m)
        {
            var builder = new StringBuilder();
            builder
                .Append("[")
                .Append(m.DeclaringType.MinimalAQName())
                .Append("]")
                .Append(".")
                .Append(m.Name)
                .Append(!m.IsGenericMethod ? "" :
                        "<" + m.GetGenericArguments().Aggregate("", (@params, param) => @params += (@params == "" ? "" : ", ") + "[" + param.MinimalAQName() + "]") + ">")
                .Append("(")
                .Append(m.GetParameters()
                         .Aggregate("", (@params, param) => @params += (@params == "" ? "" : ", ") + "[" + param.ParameterType.MinimalAQName() + "] "))
                .Append(")")
                .Append("::")
                .Append("[").Append(m.ReturnType.MinimalAQName()).Append("]");

            return builder.ToString();
        }

        private static string AccessorSignature(this PropertyInfo pinfo)
        => $"[{pinfo.DeclaringType.MinimalAQName()}].Get{pinfo.Name}";

        private static string MutatorSignature(this PropertyInfo pinfo)
        => $"[{pinfo.DeclaringType.MinimalAQName()}].Set{pinfo.Name}";

        ////private static string AccessorSignature(this FieldInfo finfo)
        ////=> $"[{finfo.DeclaringType.MinimalAQName()}].@{finfo.Name}";
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
        /// NOTE: his will fail if <c>genericDefinitionBaseType</c> is the <c>GenericTypeDefinition</c> of <c>type</c>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericDefinitionBaseType"></param>
        /// <returns></returns>
        public static bool HasGenericBaseType(this Type type, Type genericDefinitionBaseType)
        {
            if (!genericDefinitionBaseType.IsGenericTypeDefinition) throw new System.Exception("ancestor is not a generic type definition");
            return type
                .BaseTypes()
                .Where(_bt => _bt != type)
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericDefinitionBaseType)
                .Any();
        }

        public static bool ImplementsGenericInterface(this
            Type type,
            Type genericDefinitionInterfaceType)
            => type
                .GetInterfaces()
                .Any(_i => _i.IsGenericType && _i.GetGenericTypeDefinition() == genericDefinitionInterfaceType);


        public static Type GetGenericBase(this Type type, Type genericBaseDefinition)
        {
            genericBaseDefinition
                .ThrowIf(t => !t.IsGenericTypeDefinition, new ArgumentException("base type is not a generic type definition"))
                .ThrowIf(t => !t.IsClass, new ArgumentException($"supplied {nameof(genericBaseDefinition)} type is not a class"));

            return type
                .BaseTypes()
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericBaseDefinition)
                .FirstOrDefault();
        }

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

        public static Type GetGenericInterface(this Type type, Type genericDefinitionInterface)
        {
            genericDefinitionInterface
                .ThrowIf(t => !t.IsGenericTypeDefinition, new ArgumentException("interface is not a generic type definition"))
                .ThrowIf(t => !t.IsInterface, new ArgumentException($"supplied {nameof(genericDefinitionInterface)} type is not an interface"));

            return type
                .GetInterfaces()
                .Where(_i => _i.IsGenericType)
                .Where(_i => _i.GetGenericTypeDefinition() == genericDefinitionInterface)
                .FirstOrDefault();
        }

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
        /// <param name="firstInterface">The first interface to check for implementation</param>
        /// <param name="otherInterfaces">Multiple interfaces to check for implementation</param>
        public static bool Implements(this Type type, Type firstInterface, params Type[] otherInterfaces)
        {
            var interfaces = new HashSet<Type>(type.GetInterfaces());
            return firstInterface
                .EnumerateWith(otherInterfaces)
                .Distinct()
                .Select(@interface => !@interface.IsInterface
                    ? throw new InvalidOperationException($"{@interface} is not an interface")
                    : @interface)
                .All(interfaces.Contains);
        }

        /// <summary>
        /// Verifies that the given type implements any of the supplied interfaces.
        /// </summary>
        /// <param name="type">The type to test against. Can be a class or struct, or interface, etc.</param>
        /// <param name="firstInterface">The first interface to check for implementation</param>
        /// <param name="otherInterfaces">Multiple interfaces to check for implementation</param>
        public static bool ImplementsAny(this Type type, Type firstInterface, params Type[] otherInterfaces)
        {
            var interfaces = new HashSet<Type>(type.GetInterfaces());
            return firstInterface
                .EnumerateWith(otherInterfaces)
                .Distinct()
                .Select(@interface => !@interface.IsInterface
                    ? throw new InvalidOperationException($"{@interface} is not an interface")
                    : @interface)
                .Any(interfaces.Contains);
        }

        /// <summary>
        /// Ensures that a type inherits from all the listed types. If any of the bases is a GenericTypeDefinition, the method checks that any of the
        /// target type's generic bases has a generic definition that matches the given one. If the type itself is contained in the base list, it still passes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <param name="otherBases"></param>
        /// <returns></returns>
        public static bool Extends(this
            Type type,
            Type baseType,
            params Type[] otherBases)
            => type.Extends(baseType.EnumerateWith(otherBases).ToArray());

        /// <summary>
        /// Ensures that a type inherits from all the listed types. If any of the bases is a GenericTypeDefinition, the method checks that any of the
        /// target type's generic bases has a generic definition that matches the given one.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="bases"></param>
        /// <returns></returns>
        public static bool Extends(this Type type, params Type[] bases)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (bases == null || bases.Length == 0)
                return false;

            var regularBases = new HashSet<Type>();
            var genericDefinitionBases = new HashSet<Type>();

            bases.ForAll(@base =>
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

        public static IEnumerable<Type> TypeLineage(this Type type) => type.GetInterfaces().Concat(type.BaseTypes());

        /// <summary>
        /// Returns this type, along with all the types it inherits from, all the way back to "object"
        /// </summary>
        /// <param name="type"></param>
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

        #region Property

        public static bool IsInit(this PropertyInfo property)
        {
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            if (property.SetMethod is null)
                return false;

            return property.SetMethod.ReturnParameter
                .GetRequiredCustomModifiers()
                .Contains(typeof(System.Runtime.CompilerServices.IsExternalInit));
        }

        public static MemberInfo Member(Expression<Func<object>> expr)
        {
            if (!(expr is LambdaExpression lambda)) return null;
            else if (lambda.Body is UnaryExpression)
            {
                var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                return member?.Member as MemberInfo;
            }
            else if (lambda.Body is MemberExpression)
            {
                var member = (lambda.Body as MemberExpression);
                return member?.Member as MemberInfo;
            }
            else return null;
        }

        public static MemberInfo Member<V>(Expression<Func<V>> expr)
        {
            if (!(expr is LambdaExpression lambda)) return null;
            else if (lambda.Body is UnaryExpression)
            {
                if (!((lambda.Body as UnaryExpression).Operand is MemberExpression member)) return null;
                else return member.Member as MemberInfo;
            }
            else if (lambda.Body is MemberExpression)
            {
                var member = (lambda.Body as MemberExpression);
                if (member == null) return null;
                else return member.Member as MemberInfo;
            }
            else return null;
        }

        public static PropertyInfo Property<V>(Expression<Func<V>> expr) => Member(expr).As<PropertyInfo>();

        public static PropertyInfo Property(this object obj, string property) => obj?.GetType()?.GetProperty(property);


        public static V PropertyValue<V>(this object obj, string property)
        {
            if (!obj.TryGetPropertyValue<V>(property, out V val)) throw new System.Exception("Property value could not be retrieved");
            else return val;
        }

        public static object PropertyValue(this object obj, string property)
        {
            if (!obj.TryGetPropertyValue(property, out object val)) throw new System.Exception("Property value could not be retrieved");
            else return val;
        }


        public static bool TryGetPropertyValue<V>(this object obj, string property, out V val)
        {
            val = default; //<-- initial value
            var propInfo = obj.Property(property);
            if (propInfo == null) return false;
            else
            {
                try
                {
                    val = (V)GetPropertyAccessorDelegate(obj, propInfo.Name).Invoke(obj);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool TryGetPropertyValue(this object obj, string property, out object val)
        {
            val = null; //<-- initial value
            var propInfo = obj.Property(property);
            if (propInfo == null) return false;
            else
            {
                try
                {
                    val = GetPropertyAccessorDelegate(obj, propInfo.Name).Invoke(obj);
                    return true;
                }
                catch(Exception e)
                {
                    return false;
                }
            }
        }


        public static object SetPropertyValue(this object obj, string propertyName, object value)
        {
            var propInfo = obj.Property(propertyName);
            GetPropertyMutatorDelegate(obj, propertyName).Invoke(obj, value);

            return value;
        }

        public static V SetPropertyValue<V>(this object obj, Expression<Func<V>> propertyExpression, V value)
        => obj.SetPropertyValue(Property(propertyExpression).Name, value);

        public static V SetPropertyValue<V>(this object obj, string propertyName, V value) 
        => (V)obj.SetPropertyValue(propertyName, (object)value);

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
            var producer = TypeDefaultsProducer.GetOrAdd(type, _t =>
            {
                var @default = Expression.Default(_t);
                var cast = Expression.Convert(@default, typeof(object));
                var lambda = Expression.Lambda(typeof(Func<object>), cast);
                return lambda.Compile().As<Func<object>>();
            });

            return producer.Invoke();
        }

        public static bool IsPropertyAccessor(this MethodInfo method)
        => method.DeclaringType.GetProperties().Any(prop => prop.GetGetMethod() == method);

        public static bool IsPropertyMutator(this MethodInfo method)
        => method.DeclaringType.GetProperties().Any(prop => prop.GetSetMethod() == method);

        public static bool IsIntegral(this Type type) => Integrals.Contains(type);

        public static bool IsDecimal(this Type type) => Decimals.Contains(type);

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

        internal static readonly IEnumerable<Type> Decimals = new HashSet<Type>()
        {
            typeof(decimal),
            typeof(float),
            typeof(double)
        };

        #endregion

        private static Func<object, object> GetPropertyAccessorDelegate(object obj, string propertyName)
        {
            var targetType = obj.GetType();
            var property = targetType
                .GetProperty(propertyName)
                ?? throw new ArgumentException($"Invalid property name: {propertyName}");

            return PropertyAccessors
                .GetOrAdd($"{property.AccessorSignature()}", _sig =>
                {
                    var targetArg = Expression.Parameter(typeof(object), "target");
                    var targetCast = Expression.Convert(targetArg, targetType);
                    var propertyAccessor = Expression.MakeMemberAccess(targetCast, property);
                    Expression body = property.PropertyType.IsValueType
                        ? Expression.Convert(propertyAccessor, typeof(object))
                        : propertyAccessor.As<Expression>();
                    var lambda = Expression.Lambda(typeof(Func<object, object>), body, targetArg);
                    var @delegate = lambda.Compile();

                    return @delegate;
                })
                .As<Func<object, object>>();
        }

        private static Action<object, object> GetPropertyMutatorDelegate(object obj, string propertyName)
        {
            var targetType = obj.GetType();
            var property = targetType
                .GetProperty(propertyName)
                ?? throw new ArgumentException($"Invalid property name: {propertyName}");

            return PropertyAccessors
                .GetOrAdd($"{property.MutatorSignature()}", _sig =>
                {
                    var targetArg = Expression.Parameter(typeof(object), "target");
                    var valueArg = Expression.Parameter(typeof(object), "value");
                    var targetCast = Expression.Convert(targetArg, targetType);
                    var valueCast = Expression.Convert(valueArg, property.PropertyType);
                    var propertyMutatorFunction = Expression.Call(targetCast, property.GetSetMethod(), valueCast);
                    var lambda = Expression.Lambda(typeof(Action<object, object>), propertyMutatorFunction, targetArg, valueArg);
                    var @delegate = lambda.Compile();

                    return @delegate;
                })
                .As<Action<object, object>>();
        }

        private static IEnumerable<Type> FlattenIfGeneric(Type type)
        {
            yield return type;

            if (type.IsGenericType)
                yield return type.GetGenericTypeDefinition();
        }
    }
}
