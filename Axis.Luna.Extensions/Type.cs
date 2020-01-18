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

        private static string AccessorSignature(this FieldInfo finfo)
        => $"[{finfo.DeclaringType.MinimalAQName()}].@{finfo.Name}";
        #endregion


        #region Inheritance

        public static bool HasGenericAncestor(this Type type, Type genericDefinitionAncestorType)
        {
            if (!genericDefinitionAncestorType.IsGenericTypeDefinition) throw new System.Exception("ancestor is not a generic type definition");
            return type
                .BaseTypes()
                .Where(_bt => _bt != type)
                .Where(_bt => _bt.IsGenericType)
                .Where(_bt => _bt.GetGenericTypeDefinition() == genericDefinitionAncestorType)
                .Any();
        }

        public static bool ImplementsGenericInterface(this Type type, Type genericDefinitionInterfaceType)
        => type.GetInterfaces().Any(_i => _i.IsGenericType && _i.GetGenericTypeDefinition() == genericDefinitionInterfaceType);

        public static bool Implements(this Type type, Type firstInterface, params Type[] implementedInterfaces)
        {
            var interfaces = type.GetInterfaces();
            return firstInterface.Enumerate()
                .Union(implementedInterfaces)
                .Where(@interface => @interface.IsInterface)
                .All(interfaces.Contains);
        }

        /// <summary>
        /// Ensures that a type inherits from all the listed types
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <param name="otherBases"></param>
        /// <returns></returns>
        public static bool Extends(this Type type, Type baseType, params Type[] otherBases)
        => type
            .ThrowIfNull(new ArgumentNullException(nameof(type)))
            .ThrowIf(t => t.IsInterface, new ArgumentException($"argument '{nameof(type)}' cannot be an interface"))
            .BaseTypes()
            .ContainsAll(otherBases
                .Concat(baseType.Enumerate())
                .Where(t => !t.IsInterface));

        public static IEnumerable<Type> TypeLineage(this Type type) => type.GetInterfaces().Concat(type.BaseTypes());

        public static IEnumerable<Type> BaseTypes(this Type type)
        {
            Type @base = type;
            while (@base != null)
            {
                yield return @base;
                @base = @base.BaseType;
            }
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
                var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                if (member == null) return null;
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

        #endregion


        #region Property access

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
    }
}
