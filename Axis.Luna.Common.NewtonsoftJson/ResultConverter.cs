using Axis.Luna.Common.Types.Basic;
using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace Axis.Luna.Common.NewtonsoftJson
{
    /// <summary>
    /// Newtonsoft converter for the <see cref="IResult{TData}"/> instances.
    /// NOTE: this converter depends on the <see cref="BasicStructJsonConverter"/>
    /// </summary>
    public class ResultConverter : JsonConverter
    {
        private readonly bool _isExceptionExported;

        public static readonly string ExceptionJsonFieldName = "Exception";

        public ResultConverter(bool isExceptionExported = false)
        {
            _isExceptionExported = isExceptionExported;
        }

        public override bool CanConvert(Type objectType) 
            => objectType.IsGenericType
            && !objectType.IsGenericTypeDefinition
            && (objectType.ImplementsGenericInterface(typeof(IResult<>))
            || objectType.GetGenericTypeDefinition().Equals(typeof(IResult<>)));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);

            if (IsErrorResultType(objectType, out var resultType) 
                && TryExtractErrorResult(jobj, resultType, serializer, out var errorResult))
                return errorResult;

            if (IsDataResultType(objectType, out resultType) 
                && TryExtractDataResult(jobj, resultType, serializer, out var dataResult))
                return dataResult;

            if (IsResultInterface(objectType, out resultType))
            {
                if (TryExtractErrorResult(jobj, resultType, serializer, out errorResult))
                    return errorResult;

                if (TryExtractDataResult(jobj, resultType, serializer, out dataResult))
                    return dataResult;
            }

            throw new Exception($"Could not read the type {objectType}");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                throw new InvalidOperationException($"cannot write null {typeof(IResult<>)} value");

            var valueType = value.GetType();
            var resultType = ExtractResultType(valueType);
            var toJobjMethod = ResultConverter
                .GetToJObjectMethod()
                .MakeGenericMethod(resultType);

            if (valueType.Name.Equals(typeof(IResult<>.ErrorResult).Name)
                || valueType.Name.Equals(typeof(IResult<>.DataResult).Name))
                toJobjMethod
                    .InvokeFunc(value, _isExceptionExported, serializer)
                    .As<JObject>()
                    .WriteTo(writer);

            else throw new InvalidOperationException($"Invalid result type encountered: {valueType}");
        }

        private static bool IsErrorResultType(Type type, out Type resultType)
        {
            if (type.Name.Equals(typeof(IResult<>.ErrorResult).Name)
                && type.IsGenericType
                && type.DeclaringType != null
                && type.DeclaringType.Equals(typeof(IResult<>)))
            {
                resultType = type.GetGenericArguments()[0];
                return true;
            }
            else
            {
                resultType = null;
                return false;
            }
        }

        private static bool IsDataResultType(Type type, out Type resultType)
        {
            if (type.Name.Equals(typeof(IResult<>.DataResult).Name)
                && type.IsGenericType
                && type.DeclaringType != null
                && type.DeclaringType.Equals(typeof(IResult<>)))
            {
                resultType = type.GetGenericArguments()[0];
                return true;
            }
            else
            {
                resultType = null;
                return false;
            }
        }

        private static bool IsResultInterface(Type type, out Type resultType)
        {
            if (type.IsGenericType
                && !type.IsGenericTypeDefinition
                && type.GetGenericTypeDefinition() == typeof(IResult<>))
            {
                resultType = type.GetGenericArguments()[0];
                return true;
            }
            else
            {
                resultType = null;
                return false;
            }
        }

        private static bool TryExtractErrorResult(
            JObject jobj,
            Type resultType,
            JsonSerializer serializer,
            out object errorResult)
        {
            errorResult = null;

            string errorMessage = null;
            if (jobj.TryGetValue(nameof(IResult<int>.ErrorResult.Message), out var messageToken))
                errorMessage = messageToken.Value<string>();
            else
                return false;

            var errorData = jobj.TryGetValue(nameof(IResult<int>.ErrorResult.ErrorData), out var errorDataToken)
                ? errorDataToken.ToObject<BasicStruct>(serializer)
                : (BasicStruct?)null;

            var exception = new DeserializedException(errorMessage);
            if (errorData != null)
                exception.WithErrorData(errorData.Value);

            errorResult = new Func<MethodInfo>(GetErrorResultInitializerMethod<int>).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(resultType)
                .InvokeFunc()
                .As<MethodInfo>()
                .InvokeFunc(exception);
            return true;
        }

        private static bool TryExtractDataResult(
            JObject jobj,
            Type resultType,
            JsonSerializer serializer,
            out object dataResult)
        {
            dataResult = null;

            object data = null;
            if (jobj.TryGetValue(nameof(IResult<int>.DataResult.Data), out var dataToken))
                data = dataToken.ToObject(resultType, serializer);
            else
                return false;

            dataResult = new Func<MethodInfo>(GetDataResultInitializerMethod<int>).Method
                .GetGenericMethodDefinition()
                .MakeGenericMethod(resultType)
                .InvokeFunc()
                .As<MethodInfo>()
                .InvokeFunc(data);
            return true;
        }

        private static Type ExtractResultType(Type type)
        {
            if (IsResultInterface(type, out var resultType))
                return resultType;

            if (IsErrorResultType(type, out resultType))
                return resultType;

            if (IsDataResultType(type, out resultType))
                return resultType;

            else return null;
        }

        private static JObject ToJObject<T>(IResult<T> result, bool exportException, JsonSerializer serializer)
        {
            return result switch
            {
                IResult<T>.ErrorResult error => new JObject()
                    .With(
                        jobj => jobj.Add(nameof(error.Message),
                        error.Message))
                    .WithIf(
                        jobj => error.ErrorData!=null,
                        jobj => jobj.Add(
                            nameof(error.ErrorData),
                            GetBasicStructConverter(serializer).ToJObject(error.ErrorData.Value)))
                    .WithIf(
                        jobj => exportException,
                        jobj => jobj.Add(
                            ExceptionJsonFieldName,
                            JObject.FromObject(error.Cause(), serializer))),

                IResult<T>.DataResult data => JObject.FromObject(data),

                _ => throw new InvalidOperationException($"Invalid result type: {result}")
            };
        }

        private static BasicStructJsonConverter GetBasicStructConverter(JsonSerializer serializer)
        {
            return serializer.Converters
                .FirstOrDefault(c => c is BasicStructJsonConverter)
                .As<BasicStructJsonConverter>()
                ?? new BasicStructJsonConverter();
        }


        private static MethodInfo GetErrorResultInitializerMethod<T>()
            => new Func<Exception, IResult<T>>(Result.Of<T>).Method;

        private static MethodInfo GetDataResultInitializerMethod<T>() => new Func<T, IResult<T>>(Result.Of<T>).Method;

        private static MethodInfo GetToJObjectMethod()
        {
            return new Func<IResult<int>, bool, JsonSerializer, JObject>(ToJObject)
                .Method
                .GetGenericMethodDefinition();
        }


        public class DeserializedException: Exception
        {
            public DeserializedException(string message)
                : base(message)
            { }
        }
    }
}
