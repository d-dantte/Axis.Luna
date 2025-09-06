using System.Reflection;
using System.Collections.Concurrent;

namespace GridLuck.Common.Extensions
{
    public static class Common
    {
        private static readonly ConcurrentDictionary<MethodInfo, Delegate> Converters = new();
        private static readonly ConcurrentDictionary<(Type, Type), Delegate> ConverterProxies = new();
        private static readonly MethodInfo ConverterProxyMethod = typeof(Common)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(minfo => nameof(Convert).Equals(minfo.Name))
            .Where(minfo => minfo.IsGenericMethodDefinition)
            .Where(minfo => minfo.GetGenericArguments().Length == 2)
            .First();

        private const string ExplicitOperatorName = "op_Explicit";
        private const string ImplicitOperatorName = "op_Implicit";

        #region Apply
        public static TOut ApplyTo<TIn, TOut>(this TIn @in, Func<TIn, TOut> mapper)
        {
            ArgumentNullException.ThrowIfNull(mapper);
            return mapper.Invoke(@in);
        }

        public static void Consume<TIn>(this TIn @in, Action<TIn> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            action.Invoke(@in);
        }

        public static Task ConsumeAsync<TIn>(this TIn @in, Func<TIn, Task> asyncAction)
        {
            ArgumentNullException.ThrowIfNull(asyncAction);
            return asyncAction.Invoke(@in);
        }

        public static TIn With<TIn>(this TIn @in, Action<TIn> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            action.Invoke(@in);
            return @in;
        }

        public static async Task<TIn> WithAsync<TIn>(this TIn @in, Func<TIn, Task> asyncAction)
        {
            ArgumentNullException.ThrowIfNull(asyncAction);
            await asyncAction.Invoke(@in);
            return @in;
        }
        #endregion

        #region Type Check
        public static bool Is<T>(this object? value, out T t)
        {
            if (value is T __t)
            {
                t = __t;
                return true;
            }
            else
            {
                t = default!;
                return false;
            }
        }

        public static bool Is<T>(this ValueType value, out T t)
        {
            if (value is T __t)
            {
                t = __t;
                return true;
            }
            else
            {
                t = default!;
                return false;
            }
        }

        public static bool IsNot<T>(this object? value) => value is not T;

        public static bool IsBoxed(this object? value)
        {
            if (value is null)
                return false;

            else return value.GetType().IsValueType;
        }

        public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(default, value);
        #endregion

        #region Type Cast
        public static T As<T>(this object? value)
        {
            if (value is T t)
                return t;

            else if (value.TryCast<T>(out var result))
                return result;

            return default!;
        }

        public static bool TryCast<T>(this object? value, out T result)
        {
            var valueType = value?.GetType();
            var returnType = typeof(T);

            if (value is null)
            {
                result = default!;
                return false;
            }

            // value type is return type
            if (returnType == valueType)
            {
                result = (T)value;
                return true;
            }

            // value implements the return interface
            if (returnType.IsInterface && valueType!.ImplementsAll(returnType))
            {
                result = (T)value;
                return true;
            }

            // value is a subclass of the return class
            if (returnType.IsClass && valueType!.IsDescendantOf(returnType))
            {
                result = (T)value;
                return true;
            }

            // convertible
            if (value is IConvertible cvtb && returnType.ImplementsAll(typeof(IConvertible)))
            {
                result = (T)cvtb.ToType(returnType, null);
                return true;
            }

            // explicit conversion to
            if (valueType!.TryGetExplicitConverterTo<T>(out var converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // explicit conversion from
            if (returnType.TryGetExplicitConverterFrom(valueType!, out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // implicit conversion to
            if (valueType!.TryGetImplicitConverterTo<T>(out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            // implicit conversion from
            if (returnType.TryGetImplicitConverterFrom(valueType!, out converterMethod))
            {
                result = value.Convert<T>(converterMethod);
                return true;
            }

            result = default!;
            return false;
        }

        #region Explicit Converters
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

        public static bool TryGetExplicitConverterTo<TOut>(this
            Type sourceType,
            out MethodInfo converter)
            => sourceType.TryGetExplicitConverterTo(typeof(TOut), out converter);

        public static bool TryGetExplicitConverterTo(this
            Type sourceType,
            Type destinationType,
            out MethodInfo converter)
        {
            ArgumentNullException.ThrowIfNull(sourceType);

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
                .FirstOrDefault()!;

            return converter != null;
        }

        public static bool TryGetExplicitConverterFrom<TIn>(this
            Type destinationType,
            out MethodInfo converter)
            => destinationType.TryGetExplicitConverterFrom(typeof(TIn), out converter);

        public static bool TryGetExplicitConverterFrom(this
            Type destinationType,
            Type sourceType,
            out MethodInfo converter)
        {
            ArgumentNullException.ThrowIfNull(destinationType);

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
                .FirstOrDefault()!;

            return converter != null;
        }
        #endregion

        #region Implicit Converters
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

        public static bool TryGetImplicitConverterTo<TOut>(this
            Type sourceType,
            out MethodInfo converter)
            => sourceType.TryGetImplicitConverterTo(typeof(TOut), out converter);

        public static bool TryGetImplicitConverterTo(this
            Type sourceType,
            Type destinationType,
            out MethodInfo converter)
        {
            ArgumentNullException.ThrowIfNull(sourceType);

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
                .FirstOrDefault()!;

            return converter != null;
        }

        public static bool TryGetImplicitConverterFrom<TIn>(this
            Type destinationType,
            out MethodInfo converter)
            => destinationType.TryGetImplicitConverterFrom(typeof(TIn), out converter);

        public static bool TryGetImplicitConverterFrom(this Type destinationType, Type sourceType, out MethodInfo converter)
        {
            ArgumentNullException.ThrowIfNull(destinationType);

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
                .FirstOrDefault()!;

            return converter != null;
        }
        #endregion

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
        #endregion

        #region String
        public static string JoinUsing(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list);
        }

        public static string JoinUsing(this IEnumerable<string> list, char separator)
        {
            return string.Join(separator, list);
        }
        #endregion
    }
}