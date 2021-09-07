using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

namespace Axis.Luna.Operation.NewtonsoftJson
{
    public class OperationJsonConverter : global::Newtonsoft.Json.JsonConverter
    {
        public static readonly string ResultJsonPropertyName = "Result";

        private static readonly MethodInfo ReadOperationMethod = typeof(OperationJsonConverter)
            .GetMethod(
                nameof(ReadOperation),
                BindingFlags.NonPublic
                | BindingFlags.Instance);

        private static readonly MethodInfo WriteOperationMethod = typeof(OperationJsonConverter)
            .GetMethod(
                nameof(WriteOperation),
                BindingFlags.NonPublic
                | BindingFlags.Instance);

        public override bool CanConvert(Type objectType)
            => objectType.Extends(typeof(Operation)) || objectType.Extends(typeof(Operation<>));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var operationToken = JObject.Load(reader);

            if (objectType.Extends(typeof(Operation<>)))
            {
                //get the generic type of the operation, then call the "ReadOperation" method dynamically.
                var genericType = objectType
                    .GetGenericBase(typeof(Operation<>))
                    .GetGenericArguments()
                    [0];

                var readOperationMethod = ReadOperationMethod.MakeGenericMethod(genericType);

                return this.InvokeFunc(
                    readOperationMethod,
                    operationToken,
                    JsonSerializer.Create(Constants.JsonSettings));
            }
            else
            {
                var isSucceeded = operationToken.Value<bool>(nameof(Operation.Succeeded));
                if (isSucceeded)
                    return Operation.FromVoid();

                else
                {
                    if (operationToken.TryGetValue(nameof(Operation.Error), out var error))
                        return error != null
                            ? Operation.Fail(error.ToObject<OperationError>(JsonSerializer.Create(Constants.JsonSettings)))
                            : Operation.Fail();

                    else return Operation.Fail();
                }
            }
        }

        private Operation<TResult> ReadOperation<TResult>(JObject operationToken, JsonSerializer serializer)
        {
            var isSucceeded = operationToken.Value<bool>(nameof(Operation.Succeeded));
            if (isSucceeded)
            {
                if (operationToken.TryGetValue(ResultJsonPropertyName, out var result))
                    return Operation.FromResult(result.ToObject<TResult>(serializer));

                else
                    return Operation.FromResult<TResult>(default);
            }
            else
            {
                var errorProp = nameof(Operation.Error);
                if (operationToken.TryGetValue(errorProp, out var error))
                    return error != null
                        ? Operation.Fail<TResult>(error.ToObject<OperationError>(JsonSerializer.Create(Constants.JsonSettings)))
                        : Operation.Fail<TResult>();

                else return Operation.Fail<TResult>();
            }
        }

        /// <summary>
        /// Writes the operation object. Synthesizes a "Result" property on the operation json output to keep any available results.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();

            else if(value.GetType().Extends(typeof(Operation<>)))
            {
                var genericType = value
                    .GetType()
                    .GetGenericArguments()
                    [0];

                var synthesizedMethod = WriteOperationMethod.MakeGenericMethod(genericType);
                this.InvokeAction(
                    synthesizedMethod,
                    writer,
                    value,
                    serializer);
            }
            else
            {
                OperationJsonConverter
                    .ToJObject(value as Operation, serializer)
                    .WriteTo(writer);
            }
        }

        private void WriteOperation<TResult>(JsonWriter writer, Operation<TResult> operation, JsonSerializer serializer)
        {
            OperationJsonConverter
                .ToJObject(operation, serializer)
                .WriteTo(writer);
        }

        public static JObject ToJObject(Operation operation, JsonSerializer serializer)
        {
            JObject joperation = new JObject
            {
                [nameof(Operation.Succeeded)] = operation.Succeeded
            };

            if (operation.Error != null)
                joperation[nameof(Operation.Error)] = OperationErrorJsonConverter.ToJObject(operation.Error, serializer);

            return joperation;
        }

        public static JToken ToJObject<TResult>(Operation<TResult> operation, JsonSerializer serializer)
        {
            var resolvable = operation as IResolvable<TResult>;
            TResult result = default;

            if (operation.Succeeded == null)
                resolvable.TryResolve(out result, out var _);

            JObject joperation = new JObject
            {
                [nameof(Operation.Succeeded)] = operation.Succeeded
            };

            if (operation.Succeeded == true)
            {
                joperation[ResultJsonPropertyName] = result != null
                    ? JObject.FromObject(result) as JToken
                    : JValue.CreateNull();
            }
            else 
                joperation[nameof(Operation.Error)] = OperationErrorJsonConverter.ToJObject(operation.Error, serializer);

            return joperation;
        }

    }
}
