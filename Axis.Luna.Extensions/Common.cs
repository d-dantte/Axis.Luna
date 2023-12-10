using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Extensions
{

    //[DebuggerStepThrough]
    public static class Common
    {
        private static readonly ConcurrentDictionary<MethodInfo, Delegate> Converters = new ConcurrentDictionary<MethodInfo, Delegate>();
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> ConverterProxies = new ConcurrentDictionary<(Type, Type), Delegate>();
        private static readonly MethodInfo ConverterProxyMethod = typeof(Common)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(minfo => nameof(Convert).Equals(minfo.Name))
            .Where(minfo => minfo.IsGenericMethodDefinition)
            .Where(minfo => minfo.GetGenericArguments().Length == 2)
            .First();

        #region Reboxing
        private static readonly ConcurrentDictionary<Type, Delegate> ReboxerMap = new ConcurrentDictionary<Type, Delegate>();

        /// <summary>
        /// Copies the struct from the <paramref name="newValue"/> into the value pointed to by the <paramref name="boxedValue"/> "boxed" reference.
        /// This means, the boxed value will essentially be identical to the supplied argument after this call returns.
        /// </summary>
        /// <typeparam name="TValueType"></typeparam>
        /// <param name="boxedValue"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static object ReboxAs<TValueType>(this object boxedValue, TValueType newValue)
        where TValueType : struct
        {
            if (boxedValue is null)
                throw new ArgumentNullException(nameof(boxedValue));

            if (boxedValue is not TValueType)
                throw new ArgumentException(
                    $"Type mismatch. Expected: '{typeof(TValueType)}', Actual: '{boxedValue.GetType()}'");

            var reboxer = ReboxerMap
                .GetOrAdd(typeof(TValueType), _ => BuildReboxer<TValueType>())
                .As<Action<object, TValueType>>();

            reboxer.Invoke(boxedValue, newValue);
            return boxedValue;
        }

        private static Delegate BuildReboxer<TValueType>()
        where TValueType : struct
        {
            var valueType = typeof(TValueType);
            var guid = Guid
                .NewGuid()
                .ToString()
                .Replace("-", "_");

            var dynamicMethod = new DynamicMethod(
                name: $"Reboxer_For_{valueType.Name}_{guid}",
                returnType: typeof(void),
                parameterTypes: new[] { typeof(object), valueType },
                m: typeof(Common).Module);

            var emitter = dynamicMethod.GetILGenerator();

            emitter.Emit(OpCodes.Ldarg_0);           // object
            emitter.Emit(OpCodes.Unbox, valueType);  // TValueType&
            emitter.Emit(OpCodes.Ldarg_1);           // TValueType (argument value)
            emitter.Emit(OpCodes.Stobj, valueType);  // stobj !!TValueType
            emitter.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate(typeof(Action<object, TValueType>));
        }
        #endregion

        #region State Relocation
        private static readonly ConcurrentDictionary<Type, Delegate> RelocateStateMap = new ConcurrentDictionary<Type, Delegate>();

        /// <summary>
        /// Copies the state (fields) from the <paramref name="sourceValue"/> instance into the <paramref name="destinationValue"/> instance.
        /// <para/>
        /// Note that some structures may have complex associations/dependencies beyond their local fields, and so simply copying the fields
        /// may not be enough to "clone" the object. Only use this method if you understand to a large extent, the internal workings of the
        /// class you intend to copy it's state.
        /// </summary>
        /// <typeparam name="TRefType">The type of the values to copy</typeparam>
        /// <param name="destinationValue">The destination value</param>
        /// <param name="sourceValue">The source value</param>
        /// <returns></returns>
        public static TRefType CopyStateFrom<TRefType>(this TRefType destinationValue, TRefType sourceValue)
        where TRefType : class
        {
            ArgumentNullException.ThrowIfNull(destinationValue);
            ArgumentNullException.ThrowIfNull(sourceValue);

            if (!destinationValue.GetType().Equals(sourceValue.GetType()))
                throw new InvalidOperationException(
                    $"Type mismatch. "
                    + $"source type: '{sourceValue.GetType().FullName}', "
                    + $"destination type: '{destinationValue.GetType().FullName}'");

            var rerefer = RelocateStateMap
                .GetOrAdd(typeof(TRefType), _ => BuildStateRelocator<TRefType>(destinationValue.GetType()))
                .As<Action<TRefType, TRefType>>();

            rerefer.Invoke(destinationValue, sourceValue);
            return destinationValue;
        }

        private static Delegate BuildStateRelocator<TRefType>(Type instanceType)
        where TRefType : class
        {
            var fields = instanceType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return (TRefType destination, TRefType source) =>
            {
                fields.ForAll(field =>
                {
                    var value = field.FieldAccessorFor().Invoke(source);
                    var @default = field.FieldType.DefaultValue();

                    if ((@default is null && value is not null)
                        || (@default is not null && !@default.Equals(value)))
                        field.FieldMutatorFor().Invoke(destination, value);
                });
            };
        }
        #endregion


        public static bool Is<T>(this object value) => value is T;

        public static bool Is<TIn, TOut>(this TIn @in, out TOut @out)
        {
            if (@in is TOut _out)
            {
                @out = _out;
                return true;
            }

            @out = default;
            return false;
        }

        public static bool IsNot<T>(this object value) => value is not T;

        public static bool IsNot<TIn, TOut>(this TIn @in, out TOut @out)
        {
            if (@in is TOut _out)
            {
                @out = _out;
                return false;
            }

            @out = default;
            return true;
        }

        public static bool IsBoxed(this object value)
        {
            if (value == null)
                return false;

            else return value.GetType().IsValueType;
        }

        public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(default, value);

        public static void NoOp() { }

        public static void NoOp<T>(T arg) { }

        public static T Default<T>() => default;

        public static TOut Default<TIn, TOut>(TIn @in) => default;

        public static bool NullOrEquals<T>(this
            T operand1,
            T operand2)
            => EqualityComparer<T>.Default.Equals(operand1, operand2);

        public static bool NullOrTrue<T>(this T operand1, T operand2, Func<T, T, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (operand1 is null && operand2 is null)
                return true;

            if (operand1 is null ^ operand2 is null)
                return false;

            return predicate.Invoke(operand1, operand2);
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

        public static T As<T>(this object value)
        {
            _ = value.TryCast<T>(out var result);
            return result;
        }

        public static bool TryCast<T>(this object value, out T result)
        {
            var valueType = value?.GetType();
            var returnType = typeof(T);

            if (value is null)
            {
                result = default;
                return false;
            }

            // value type is return type
            if(returnType == valueType)
            {
                result = (T)value;
                return true;
            }

            // value implements the return interface
            if (returnType.IsInterface && valueType.Implements(returnType))
            {
                result = (T)value;
                return true;
            }

            // value is a subclass of the return class
            if (returnType.IsClass && valueType.Extends(returnType))
            {
                result = (T)value;
                return true;
            }

            // convertible
            if (value is IConvertible && returnType.Implements(typeof(IConvertible)))
            {
                result = (T)(value as IConvertible).ToType(returnType, null);
                return true;
            }

            // explicit conversion to
            if (valueType.TryGetExplicitConverterTo<T>(out var converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // explicit conversion from
            if (returnType.TryGetExplicitConverterFrom(valueType, out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // implicit conversion to
            if (valueType.TryGetImplicitConverterTo<T>(out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // implicit conversion from
            if (returnType.TryGetImplicitConverterFrom(valueType, out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            result = default;
            return false;
        }

        private static TOut Convert<TOut>(this object value, MethodInfo method)
        {
            var inType = value.GetType();
            var converterDelegate = Converters.GetOrAdd(method, m =>
            {
                var funcType = typeof(Func<,>).MakeGenericType(inType, typeof(TOut));
                return Delegate.CreateDelegate(funcType, m);
            });

            var converterProxyFunc = (Func<Delegate, object, TOut>)ConverterProxies.GetOrAdd((inType, typeof(TOut)), _ =>
            {
                var proxyFunctype = typeof(Func<Delegate, object, TOut>);
                var proxyMethod = ConverterProxyMethod.MakeGenericMethod(inType, typeof(TOut));
                return Delegate.CreateDelegate(proxyFunctype, proxyMethod);
            });

            return converterProxyFunc.Invoke(converterDelegate, value);
        }

        private static TOut Convert<TIn, TOut>(this Delegate del, object value)
        {
            return ((Func<TIn, TOut>)del).Invoke((TIn)value);
        }

        public static dynamic AsDynamic(this object value) => value;

        public static bool IsStructValue(this object value) => value?.GetType().IsValueType == true;
        public static bool IsPrimitive(this object value) => value?.GetType().IsPrimitive == true;
        public static bool IsIntegral(this object value) => value?.GetType().IsIntegral() ?? false;
        public static bool IsDecimal(this object value) => value?.GetType().IsDecimal() ?? false;
        public static bool IsNull(this object value) => value == null;
        public static bool IsNotNull(this object value) => value != null;

        public static int PropertyHash(this object @this)
            => ValueHash(@this
            .GetType()
            .GetProperties()
            .OrderBy(p => p.Name)
            .Select(p => p.GetValue(@this)));

        public static int ValueHash(
            IEnumerable<object> values,
            int prime1 = 19,
            int prime2 = 181)
            => ValueHashInternal(prime1, prime2, values.ToArray());

        public static int ValueHash(
            int prime1,
            int prime2,
            params object[] values)
            => ValueHashInternal(prime1, prime2, values);

        public static int ValueHash(params
            object[] values) 
            => ValueHashInternal(19, 181, values);

        private static int ValueHashInternal(
            int prime1,
            int prime2,
            object[] values)
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

        #region ApplyTo
        public static TOut ApplyTo
            <TIn1, TIn2, TOut>
            (this (TIn1 in1, TIn2 in2) input,
             Func<TIn1, TIn2, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3) input,
             Func<TIn1, TIn2, TIn3, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4) input,
             Func<TIn1, TIn2, TIn3, TIn4, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TIn5, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5) input,
             Func<TIn1, TIn2, TIn3, TIn4, TIn5, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4, input.in5);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6) input,
             Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4, input.in5, input.in6);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7) input,
             Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4, input.in5, input.in6, input.in7);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8) input,
             Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4, input.in5, input.in6, input.in7, input.in8);
        }

        public static TOut ApplyTo
            <TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut>
            (this (TIn1 in1, TIn2 in2, TIn3 in3, TIn4 in4, TIn5 in5, TIn6 in6, TIn7 in7, TIn8 in8, TIn9 in9) input,
             Func<TIn1, TIn2, TIn3, TIn4, TIn5, TIn6, TIn7, TIn8, TIn9, TOut> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return mapper.Invoke(input.in1, input.in2, input.in3, input.in4, input.in5, input.in6, input.in7, input.in8, input.in9);
        }
        #endregion

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
                throw new ArgumentNullException(nameof(consumer));

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

        public static string Trim(this
            string @string,
            string trimChars)
            => @string.TrimStart(trimChars).TrimEnd(trimChars);

        public static string TrimStart(this
            string original,
            string searchString)
            => original.StartsWith(searchString)
               ? original[searchString.Length..]
               : original;

        public static string TrimEnd(this
            string original,
            string searchString)
            => original.EndsWith(searchString)
               ? original[..^searchString.Length]
               : original;

        public static string JoinUsing(this
            IEnumerable<string> strings,
            string separator)
            => string.Join(separator, strings);

        public static string JoinUsing(this
            IEnumerable<char> subStrings,
            string separator)
            => string.Join(separator, subStrings.ToArray());

        public static string WrapIn(this
            string @string,
            string left,
            string right = null)
            => $"{left}{@string}{right ?? left}";

        public static string WrapIf(this
            string @string, Func<string, bool> predicate,
            string left,
            string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.WrapIn(left, right);

            else return @string;
        }

        public static string UnwrapFrom(this
            string @string,
            string left,
            string right = null)
        {
            if (@string.IsWrappedIn(left, right))
                return @string
                    [left.Length..^(right ?? left).Length]; //remove the first left.length, and the last (left|right).length, characters

            else return @string;
        }

        public static string UnwrapIf(this
            string @string,
            Func<string, bool> predicate,
            string left,
            string right = null)
        {
            if (predicate.Invoke(@string))
                return @string.UnwrapFrom(left, right);

            else return @string;
        }

        public static bool IsWrappedIn(this
            string @string,
            string left,
            string right = null)
        {
            return @string.StartsWith(left) && @string.EndsWith(right ?? left);
        }

        public static int SubstringCount(this string source, string subString)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(subString)) return 0;
            else if (subString.Length > source.Length) return 0;

            int lindex = 0;
            int count = 0;
            do
            {
                var sub = source.Substring(lindex, subString.Length);

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

        public static bool ContainsAny(this
            string source,
            params string[] substrings)
            => substrings.Any(source.Contains);

        public static bool ContainsAll(this
            string source,
            params string[] substrings)
            => substrings.All(source.Contains);

        public static string SplitCamelCase(this
            string source,
            string separator = " ")
            => source
            .Aggregate(new StringBuilder(), (acc, ch) => acc.Append(char.IsUpper(ch) ? separator : "").Append(ch))
            .ToString()
            .Trim();
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
