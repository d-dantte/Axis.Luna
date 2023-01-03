using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{

    //[DebuggerStepThrough]
    public static class Common
    {
        public static bool IsBoxed<T>(this T value)
        {
            return (typeof(T).IsInterface || typeof(T) == typeof(object)) 
                && value != null
                && value.GetType().IsValueType;
        }

        public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(default, value);

        public static void NoOp() { }

        public static void NoOp<T>(T arg) { }

        public static T Default<T>() => default;

        public static TOut Default<TIn, TOut>(TIn @in) => default;

        public static bool NullOrEquals<T>(this T operand1, T operand2)
            => EqualityComparer<T>.Default.Equals(operand1, operand2);

        public static bool NullOrTrue<T>(this T operand1, T operand2, Func<T, T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (operand1 == null && operand2 == null)
                return true;

            if (operand1 != null && operand2 != null)
                return predicate.Invoke(operand1, operand2);

            return false;
        }


        /// <summary>
        /// Convenience method for "using" disposables with lambda delegates
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static Out Using<D, Out>(this D disposable, Func<D, Out> func)
        where D : IDisposable
        {
            using (disposable)
            {
                return func(disposable);
            }
        }

        /// <summary>
        /// Async version of Convenience method for "using" disposables with lambda delegates
        /// </summary>
        /// <typeparam name="D"></typeparam>
        /// <typeparam name="Out"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<Out> UsingAsync<D, Out>(this D disposable, Func<D, Task<Out>> func)
        where D : IDisposable
        {
            using (disposable)
            {
                return await func(disposable);
            }
        }

        public static void Using<D>(this D disposable, Action<D> action)
        where D : IDisposable
        {
            using (disposable)
            {
                action(disposable);
            }
        }

        public static async Task UsingAsync<D>(this D disposable, Func<D, Task> action)
        where D : IDisposable
        {
            using (disposable)
            {
                await action(disposable);
            }
        }

        public static KeyValuePair<K, V> ValuePair<K, V>(this K key, V value) => new KeyValuePair<K, V>(key, value);


        public static KeyValuePair<K, object> ObjectPair<K>(this K key, object obj) => new KeyValuePair<K, object>(key, obj);

        //[Obsolete]
        //public static T As<T>(this object value)
        //where T: class
        //{
        //    try
        //    {
        //        if (value is IConvertible 
        //            && typeof(IConvertible).IsAssignableFrom(typeof(T)))
        //            return (T)Convert.ChangeType(value, typeof(T));

        //        else return (T)value;
        //    }
        //    catch
        //    {
        //        return default;
        //    }
        //}

        //[Obsolete]
        //public static T As<S, T>(this S value)
        //{
        //    try
        //    {
        //        if (value is IConvertible 
        //            && typeof(IConvertible).IsAssignableFrom(typeof(T)))
        //            return (T)Convert.ChangeType(value, typeof(T));

        //        else 
        //            return (T)(object)value;
        //    }
        //    catch
        //    {
        //        return default;
        //    }
        //}

        public static dynamic AsDynamic(this object value) => value;

        public static bool IsStructValue(this object value) => value?.GetType().IsValueType == true;
        public static bool IsPrimitive(this object value) => value?.GetType().IsPrimitive == true;
        public static bool IsIntegral(this object value) => value?.GetType().IsIntegral() ?? false;
        public static bool IsDecimal(this object value) => value?.GetType().IsDecimal() ?? false;
        public static bool IsNull(this object value) => value == null;
        public static bool IsNotNull(this object value) => value != null;

        public static int PropertyHash(this object @this)
        => ValueHash(@this.GetType().GetProperties().OrderBy(p => p.Name).Select(p => p.GetValue(@this)));

        public static int ValueHash(IEnumerable<object> values, int prime1 = 19, int prime2 = 181)
        => ValueHashInternal(prime1, prime2, values.ToArray());

        public static int ValueHash(int prime1, int prime2, params object[] values)
        => ValueHashInternal(prime1, prime2, values);

        public static int ValueHash(params object[] values) 
        => ValueHashInternal(19, 181, values);

        private static int ValueHashInternal(int prime1, int prime2, object[] values)
        => values.Aggregate(prime1, (hash, next) => hash * prime2 + (next?.GetHashCode() ?? 0));


        #region Apply/Consume/Use

        /// <summary>
        /// Applies the mapping function to the given input argument
        /// </summary>
        /// <typeparam name="TIn">Input argument type</typeparam>
        /// <typeparam name="TOut">Output argument type</typeparam>
        /// <param name="in">Input argument</param>
        /// <param name="mapper">mapping function</param>
        /// <returns>transformed output</returns>
        public static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            if (mapper == null) 
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(@in);
        }

        /// <summary>
        /// Consumes the given input using the consumer action, and returns the input
        /// </summary>
        /// <typeparam name="TIn">Input argument type</typeparam>
        /// <param name="in">input argument</param>
        /// <param name="consumer">consumer function</param>
        /// <returns>The Input argument</returns>
        public static TIn With<TIn>(this TIn @in, Action<TIn> consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(Consume));

            else consumer.Invoke(@in);

            return @in;
        }

        public static TIn WithIf<TIn>(this
            TIn @in,
            Func<TIn, bool> predicate,
            Action<TIn> consumer)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer));

            if (predicate.Invoke(@in))
                consumer.Invoke(@in);

            return @in;
        }

        /// <summary>
        /// Consumes the given input using the consumer action.
        /// </summary>
        /// <typeparam name="TIn">Input argument type</typeparam>
        /// <param name="in">input argument</param>
        /// <param name="consumer">consumer function</param>
        public static void Consume<TIn>(this TIn @in, Action<TIn> consumer)
        {
            if (consumer == null)
                throw new ArgumentNullException(nameof(consumer));

            consumer.Invoke(@in);
        }
        #endregion

        #region String extensions
        public static string Trim(this string @string, string trimChars) => @string.TrimStart(trimChars).TrimEnd(trimChars);

        public static string TrimStart(this string original, string searchString)
        => original.StartsWith(searchString)
           ? original.Substring(searchString.Length)
           : original;

        public static string TrimEnd(this string original, string searchString)
        => original.EndsWith(searchString)
           ? original.Substring(0, original.Length - searchString.Length)
           : original;

        public static string JoinUsing(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

        public static string JoinUsing(this IEnumerable<char> subStrings, string separator) => string.Join(separator, subStrings.ToArray());

        public static string WrapIn(this string @string, string left, string right = null) => $"{left}{@string}{right ?? left}";
        public static string WrapIf(this string @string, Func<string, bool> predicate, string left, string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.WrapIn(left, right);

            else return @string;
        }
        public static string UnwrapFrom(this string @string, string left, string right = null)
        {
            if (@string.IsWrappedIn(left, right))
                return @string.TrimStart(left).TrimEnd(right ?? left);

            else return @string;
        }
        public static string UnwrapIf(this string @string, Func<string, bool> predicate, string left, string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.UnwrapFrom(left, right);

            else return @string;
        }
        public static bool IsWrappedIn(this string @string, string left, string right = null)
        {
            return @string.StartsWith(left) && @string.EndsWith(right ?? left);
        }

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
        public static bool ContainsAny(this string source, params string[] substrings) => substrings.Any(source.Contains);
        public static bool ContainsAll(this string source, params string[] substrings) => substrings.All(source.Contains);

        public static string SplitCamelCase(this string source, string separator = " ")
        => source.Aggregate(new StringBuilder(), (acc, ch) => acc.Append(char.IsUpper(ch) ? separator : "").Append(ch)).ToString().Trim();
        #endregion
    }


    internal class CastVector
    {
        internal Type From { get; set; }
        internal Type To { get; set; }

        public override bool Equals(object obj)
        {
            return obj is CastVector other
                && other.From == From
                && other.To == To;
        }

        public override int GetHashCode() => Common.ValueHash(From, To);
    }
}
