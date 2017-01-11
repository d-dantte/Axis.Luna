namespace Axis.Luna.Extensions
{
    using Axis.Luna.MetaTypes;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using static Axis.Luna.Extensions.TypeExtensions;

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

        public static R Eval<R>(Func<R> func, Func<Exception, R> error = null)
        {
            try
            {
                return func();
            }
            catch (Exception e)
            {
                try { return error(e); } catch { }
                return default(R);
            }
        }
        public static void Eval(Action action, Action<Exception> error = null)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                try { error(e); } catch { }
            }
        }

        public static KeyValuePair<K, V> ValuePair<K, V>(this K key, V value) => new KeyValuePair<K, V>(key, value);

        public static T As<T>(this object value) => Eval(() => (T)value);
        public static dynamic AsDynamic(this object value) => value;
        public static bool Is<T>(this object value)
            => value != null && typeof(T).IsAssignableFrom(value.GetType());

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
            foreach (var prop in tobj.GetProperties())
            {
                if (ignoredProperties.Contains(prop.Name)) continue;

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
            dynamic _exp = initializer.Is<ExpandoObject>() ? initializer : initializer.ToDynamic();
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
            => propertyValues.Aggregate(19, (hash, next) => hash * 181 + (next?.GetHashCode() ?? 0));


        public static Out Pipe<Out, In>(this In @this, Func<In, Out> projection) => projection(@this); 

        public static Out PipeIf<Out, In>(this In @this, Func<In, bool> predicate, Func<In, Out> projection)
            => predicate(@this) ? projection(@this) : default(Out);

        public static Out PipeIf<Out, In>(this In @this, Func<In, Func<In, Out>> projectionGenerator)
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


        public static @void Pipe<In>(this In v, Action<In> action) => Void(() => action(v));

        public static @void PipeIf<In>(this In v, Func<In, bool> predicate, Action<In> action)
        {
            if (predicate(v)) action(v);

            return Void();
        }

        public static @void PipeIf<In>(this In @this, Func<In, Action<In>> actionGenerator)
        {
            actionGenerator(@this)?.Invoke(@this);
            return Void();
        }

        /// <summary>
        /// pipes the primary-parameter into the action, except it (primary-parameter) is the default value of its type, 
        /// in which case, commence to returning @void
        /// </summary>
        /// <typeparam name="Out"></typeparam>
        /// <typeparam name="In"></typeparam>
        /// <param name="this"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static @void PipeOrDefault<In>(this In @this, Action<In> action)
            => EqualityComparer<In>.Default.Equals(default(In), @this) ? Void() : Void(() => action(@this));



        public static void Do<In>(this In @this, Action<In> action) => action(@this);

        public static void DoIf<In>(this In @this, Func<In, bool> predicate,  Action<In> action)
        {
            if (predicate(@this)) action(@this);
        }

        public static void DoIf<In>(this In @this, Func<In, Action<In>> actionGenerator) => actionGenerator(@this)?.Invoke(@this);
        public static void DoOrDefault<In>(this In @this, Action<In> action)
        {
            if (!EqualityComparer<In>.Default.Equals(default(In), @this)) action(@this);
        }


        private static ConcurrentDictionary<Type, Delegate> _ComparerCache = new ConcurrentDictionary<Type, Delegate>();
        public static Delegate ComparerFor(Type t)
            => _ComparerCache.GetOrAdd(t, _t =>
            {
                var eqc__t = typeof(EqualityComparer<>).MakeGenericType(t);
                var eqc__p = eqc__t.GetProperty(nameof(EqualityComparer<object>.Default)); //only need the name "Default"

               return eqc__p.GetValue(null).Method(nameof(EqualityComparer<object>.Equals), t, t); //only need the name "Equals"
            });


        public static Out SwitchDefault<Out>(this bool @switch, Out output) => @switch ? output : default(Out);


        #region @void Extensions
        public static @void Void() => Luna.Void.@void;

        public static @void Void(Action action)
        {
            action();
            return Luna.Void.@void;
        }
        #endregion

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

        public static int SubstringCount(this String source, String subString)
        {
            if (String.IsNullOrWhiteSpace(source) || String.IsNullOrWhiteSpace(subString)) return 0;
            if (subString.Length > source.Length) return 0;

            int lindex = 0;
            String sub = null;
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
    }
}
