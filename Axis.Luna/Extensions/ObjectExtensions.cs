namespace Axis.Luna.Extensions
{
    using Operation;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    [DebuggerStepThrough]
    public static class ObjectExtensions
    {

        public static Out Using<D, Out>(this D disposable, Func<D, Out> func)
        where D : IDisposable
        {
            using (disposable) return func(disposable);
        }
        public static void Using<D>(this D disposable, Action<D> action)
        where D : IDisposable
        {
            using (disposable) action(disposable);
        }

        public static Out UsingValue<Out>(this Out @this, Action<Out> action)
        {
            action(@this);
            return @this;
        }

        public static T GetRoot<T>(this T obj, Func<T, T> step)
        {
            var temp = obj;
            var eqc = EqualityComparer<T>.Default;
            while (!eqc.Equals((temp = step(temp)), default(T))) obj = temp;
            return obj;
        }


        public static KeyValuePair<K, V> ValuePair<K, V>(this K key, V value) => new KeyValuePair<K, V>(key, value);

        public static T Cast<T>(this object value)
        => ResolvedOp.Try(() =>
        {
            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(typeof(T)))
                return (T)Convert.ChangeType(value, typeof(T));
            else return (T)value;
        }).Result;
        public static T Cast<S, T>(this S value)
        => ResolvedOp.Try(() =>
        {
            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(typeof(T)))
                return (T)Convert.ChangeType(value, typeof(T));
            else return (T)(object)value;
        }).Result;

        public static dynamic AsDynamic(this object value) => value;

        public static bool IsStructural(this object value) => value?.GetType().IsValueType == true;
        public static bool IsPrimitive(this object value) => value?.GetType().IsPrimitive == true;
        public static bool IsIntegral(this object value) => value?.GetType().IsIntegral() ?? false;
        public static bool IsDecimal(this object value) => value?.GetType().IsDecimal() ?? false;

        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public enum ObjectCopyMode
        {
            /// <summary>
            /// Replace everything
            /// </summary>
            Replace,

            /// <summary>
            /// Ignores null values for objects
            /// </summary>
            IgnoreNulls,

            /// <summary>
            /// Ignores null values for objects, and defaults for value-types
            /// </summary>
            IgnoreNullsAndDefaults,

            /// <summary>
            /// copies only modified values
            /// </summary>
            CopyModified
        };

        public static Obj CopyFrom<Obj>(this Obj dest, Obj source, params string[] ignoredProperties)
        => dest.CopyFrom(source, ObjectCopyMode.Replace, ignoredProperties);

        public static Obj CopyTo<Obj>(this Obj source, Obj dest, params string[] ignoredProperties)
        => source.CopyTo(dest, ObjectCopyMode.Replace, ignoredProperties);

        /// <summary>
        /// Copy from source object to destination object, and return the destination object
        /// </summary>
        /// <typeparam name="Obj"></typeparam>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Obj CopyFrom<Obj>(this Obj dest, Obj source, ObjectCopyMode mode, params string[] ignoredProperties)
        => (source == null) ? dest : source.CopyTo(dest, mode, ignoredProperties).Pipe(x => dest);

        /// <summary>
        /// Copy from source object to destination object, and return the source object
        /// </summary>
        /// <typeparam name="Obj"></typeparam>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static Obj CopyTo<Obj>(this Obj source, Obj dest, ObjectCopyMode mode, params string[] ignoredProperties)
        {
            var tobj = typeof(Obj);
            ignoredProperties = ignoredProperties ?? new string[0];

            ///add support for ignoring the "ignored properties"
            foreach (var prop in tobj.GetProperties().Where(_p => !ignoredProperties.Contains(_p.Name)))
            {
                var svalue = prop.GetValue(source);
                if (mode == ObjectCopyMode.Replace
                    || (mode == ObjectCopyMode.IgnoreNulls && svalue != null)
                    || (mode == ObjectCopyMode.IgnoreNullsAndDefaults && svalue != prop.PropertyType.DefaultValue())
                    || (mode == ObjectCopyMode.CopyModified && !(svalue?.Equals(prop.GetValue(dest)) ?? false)))
                    prop.SetValue(dest, svalue);
            }
            return source;
        }

        public static T With<T>(this T target, object initializer)
        {
            dynamic _exp = (initializer is ExpandoObject) ? initializer : initializer.ToDynamic();
            var _init = _exp as IDictionary<string, object>;
            var _caseless = new Dictionary<string, object>().AddAll(_init.Select(kvp => kvp.Key.ToLower().ValuePair(kvp.Value)));
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(target))
            {
                if (_caseless.ContainsKey(prop.Name.ToLower()))
                    prop.SetValue(target, _caseless[prop.Name.ToLower()]);
            }
            return target;
        }

        public static int PropertyHash(this object @this, int prime1 = 19, int prime2 = 181)
        => ValueHash(@this.GetType().GetProperties().OrderBy(p => p.Name).Select(p => p.GetValue(@this)));

        public static int ValueHash(IEnumerable<object> propertyValues, int prime1 = 19, int prime2 = 181)
        => ValueHash(prime1, prime2, propertyValues.ToArray());

        public static int ValueHash(int prime1, int prime2, params object[] propertyValues)
        => propertyValues.Aggregate(prime1, (hash, next) => hash * prime2 + (next?.GetHashCode() ?? 0));

        public static int ValueHash(params object[] values) => ValueHash(values.AsEnumerable());


        public static Out Pipe<In, Out>(this In @this, Func<In, Out> projection) => projection(@this); 

        public static Out PipeIf<In, Out>(this In @this, Func<In, bool> predicate, Func<In, Out> projection) => predicate(@this) ? projection(@this) : default(Out);

        public static Out PipeIf<In, Out>(this In @this, Func<In, Func<In, Out>> projectionGenerator)
        {
            var projection = projectionGenerator(@this);
            if (projection == null) return default(Out);
            else return projection.Invoke(@this);
        }

        /// <summary>
        /// pipes the primary-parameter into the projection, except it (primary-parameter) is the default value of its type, 
        /// in which case, returns the default value of the projection's return type.
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <typeparam name="In"></typeparam>
        /// <param name="this"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static Out PipeOrDefault<Out, In>(this In @this, Func<In, Out> projection)
        => EqualityComparer<In>.Default.Equals(default(In), @this) ? default(Out) : projection(@this);


        public static void Pipe<In>(this In v, Action<In> action) => action(v);

        public static void PipeIf<In>(this In v, Func<In, bool> predicate, Action<In> action)
        {
            if (predicate(v)) action(v);
        }

        public static void PipeIf<In>(this In @this, Func<In, Action<In>> actionGenerator)
        => actionGenerator(@this)?.Invoke(@this);

        /// <summary>
        /// pipes the primary-parameter into the action, except it (primary-parameter) is the default value of its type, 
        /// in which case, commence to returning @void
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <typeparam name="In"></typeparam>
        /// <param name="this"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static void PipeOrDefault<In>(this In @this, Action<In> action)
        {
            if (EqualityComparer<In>.Default.Equals(default(In), @this)) action(@this);
        }
        

        #region String extensions
        public static string Trim(this string @string, string trimChars) => @string.TrimStart(trimChars).TrimEnd(trimChars);

        public static string TrimStart(this string original, string searchString)
        => original.StartsWith(searchString) ?
           original.Substring(searchString.Length) :
           original;

        public static string TrimEnd(this string original, string searchString)
        => original.EndsWith(searchString) ?
           original.Substring(0, original.Length - searchString.Length) :
           original;

        public static string JoinUsing(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

        public static int SubstringCount(this string source, string subString)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(subString)) return 0;
            else if (subString.Length > source.Length) return 0;

            int lindex = 0;
            string sub = null;
            int count = 0;
            do
            {
                sub = source.Substring(lindex, subString.Length);

                if (sub.Equals(subString))
                {
                    count++;
                    lindex += subString.Length;
                }
                else lindex++;
            }
            while ((lindex + subString.Length) <= source.Length);

            return count;
        }
        public static bool ContainsAny(this string source, params string[] substrings) => substrings.Any(s => source.Contains(s));
        public static bool ContainsAll(this string source, params string[] substrings) => substrings.All(s => source.Contains(s));

        public static string SplitCamelCase(this string source, string separator = " ")
        => source.Aggregate(new StringBuilder(), (acc, ch) => acc.Append(char.IsUpper(ch) ? separator : "").Append(ch)).ToString().Trim();
        #endregion

        #region Random Numbers
        public static int RandomSignedInt(this RandomNumberGenerator rng)
        {
            var intByte = new byte[4];
            rng.GetBytes(intByte);
            return BitConverter.ToInt32(intByte, 0);
        }
        public static int RandomInt(this RandomNumberGenerator rng, int minInclusive = 0, int maxExclusive = int.MaxValue)
        {
            var value = Math.Abs(rng.RandomSignedInt()) + minInclusive;

            if (value >= maxExclusive) return value % maxExclusive;
            else return value;
        }
        public static long RandomSignedLong(this RandomNumberGenerator rng, long minInclusive = 0, long maxExclusive = long.MaxValue)
        {
            var intByte = new byte[8];
            rng.GetBytes(intByte);
            return BitConverter.ToInt64(intByte, 0);
        }
        public static long RandomLong(this RandomNumberGenerator rng, long minInclusive = 0, long maxExclusive = long.MaxValue)
        {
            var value = Math.Abs(rng.RandomSignedInt()) + minInclusive;

            if (value >= maxExclusive) return value % maxExclusive;
            else return value;
        }

        #endregion
    }


    internal class CastVector
    {
        internal Type From { get; set; }
        internal Type To { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj.Cast<CastVector>();

            return other != null &&
                   other.From == From &&
                   other.To == To;
        }

        public override int GetHashCode() => ObjectExtensions.ValueHash(From, To);
    }
}
