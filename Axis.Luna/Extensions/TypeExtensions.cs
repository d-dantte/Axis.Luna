using static Axis.Luna.Extensions.ObjectExtensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;
using CSharpBinder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Axis.Luna.Extensions
{
    public static class TypeExtensions
    {
        private static ConcurrentDictionary<Type, object> TypeDefaults = new ConcurrentDictionary<Type, object>();
        private static ConcurrentDictionary<Type, string> MinimalAQNames = new ConcurrentDictionary<Type, string>();

        #region Helpers
        private static string AccessorSignature(this PropertyInfo pinfo)
            => $"[{pinfo.DeclaringType.MinimalAQName()}].Get{pinfo.Name}";
        private static string MutatorSignature(this PropertyInfo pinfo)
            => $"[{pinfo.DeclaringType.MinimalAQName()}].Set{pinfo.Name}";
        #endregion

        public static bool HasAttribute(this Type type, Type attributeType)
             => type.GetCustomAttribute(attributeType) != null;
        public static bool HasAttribute<A>(this Type type)
        where A : Attribute => type.GetCustomAttribute<A>() != null;

        public static bool HasAttribute(this MemberInfo member, Type attributeType)
             => member.GetCustomAttribute(attributeType) != null;

        public static bool HasAttribute<A>(this MemberInfo member)
        where A : Attribute => member.GetCustomAttribute<A>() != null;

        public static object DefaultValue(this Type type)
            => type.IsValueType ? TypeDefaults.GetOrAdd(type, t => Activator.CreateInstance(t)) : null;

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

        public static IEnumerable<Type> TypeLineage(this Type type) => type.GetInterfaces().Concat(type.BaseTypes());

        public static IEnumerable<Type> BaseTypes(this Type type) => type.Enumerate(t => Operation.Run(() => t.BaseType.ThrowIfNull()));

        public static MemberInfo Member(Expression<Func<object>> expr)
        {
            var lambda = expr as LambdaExpression;
            if (lambda == null) return null;
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

        #region Property access
        public static PropertyInfo Property(Expression<Func<object>> expr)
        {
            return Member(expr).As<PropertyInfo>();
        }

        public static object PropertyValue(this object obj, Expression<Func<object>> expr)
            => Property(expr).GetValue(obj);

        public static PropertyInfo Property(this object obj, string property)
        {
            return obj?.GetType()?.GetProperty(property);
        }

        public static object PropertyValue(this object obj, string property)
        {
            object val = null;
            if (!obj.TryPropertyValue(property, ref val)) throw new Exception();
            else return val;
        }

        public static V PropertyValue<V>(this object obj, string property)
        {
            return (V)obj.PropertyValue(property);
        }

        public static bool TryPropertyValue(this object obj, string property, ref object val)
        {
            var t = obj.GetType();
            var prop = t.GetProperty(property);
            if (prop == null) return false;
            else
            {

                //Create 
                // Expression<Func<ObjectType, ReturnType>> x = (obj) => obj.Property;
                //dynamically, then compile it into a delegate and cache the delegate.
                //access to the propety will be done via that delegate.

                throw new Exception("Not implemented");
            }
        }
        public static bool TryPropertyValue<V>(this object obj, string property, ref V val)
        {
            object oval = val;
            var r = obj.TryPropertyValue(property, ref oval);
            val = (V)oval;
            return r;
        }
        #endregion

        #region Field access
        public static FieldInfo Field(Expression<Func<object>> expr)
            => Member(expr).As<FieldInfo>();
        public static object FieldVaue(this object obj, Expression<Func<object>> expr)
            => Field(expr).GetValue(obj);
        public static object FieldValue(this object obj, string field)
        {
            object val = null;
            if (!obj.TryFieldValue(field, ref val)) throw new Exception();
            else return val;
        }
        public static V FieldValue<V>(this object obj, string field)
        {
            return (V)obj.FieldValue(field);
        }
        public static bool TryFieldValue(this object obj, string field, ref object val)
        {
            var t = obj.GetType();
            var f = t.GetField(field);
            if (f == null) return false;
            else
            {
                //employ the same strategy used for Property access here: delegates.
                val = f.GetValue(obj);
                return true;
            }
        }
        public static bool TryFieldValue<V>(this object obj, string field, ref V val)
        {
            object oval = val;
            var r = obj.TryFieldValue(field, ref oval); //inspired by Grand Empress of the Dantasia Realm - Nancy Damasus ;).
            val = (V)oval;
            return r;
        }
        #endregion

        public static bool Implements(this Type type, Type firstInterface, params Type[] implementedInterfaces)
        {
            var interfaces = type.GetInterfaces();
            return firstInterface.Enumerate().Union(implementedInterfaces)
                                             .Where(intf => intf.IsInterface)
                                             .All(intf => interfaces.Contains(intf));
        }


        public static bool IsIntegral(this Type type) => Integrals.Contains(type);

        public static bool IsDecimal(this Type type) => Decimals.Contains(type);

        public static IEnumerable<Type> Integrals = new HashSet<Type>()
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

        public static IEnumerable<Type> Decimals = new HashSet<Type>()
        {
            typeof(decimal),
            typeof(float),
            typeof(double)
        };
    }
}
