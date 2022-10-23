using Axis.Luna.Extensions;
using Axis.Luna.FInvoke;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Axis.Luna.Operation.NewtonsoftJson
{
    [Obsolete]
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
            => objectType is IOperation
            || objectType.Implements(typeof(IOperation))
            || objectType.HasGenericInterfaceDefinition(typeof(IOperation<>))
            || objectType.ImplementsGenericInterface(typeof(IOperation<>));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var operationToken = JObject.Load(reader);

            if(objectType is IOperation || objectType.Implements(typeof(IOperation)))
            {
                var isSucceeded = operationToken.Value<bool>(nameof(IOperation.Succeeded));
                if (isSucceeded)
                    return Operation.FromVoid();

                else
                {
                    if (operationToken.TryGetValue(nameof(IOperation.Error), out var error))
                        return error != null
                            ? Operation.Fail(error.ToObject<OperationError>(serializer)) //JsonSerializer.Create(Constants.JsonSettings)))
                            : Operation.Fail();

                    else return Operation.Fail();
                }
            }
            else
            {
                //get the generic type of the operation, then call the "ReadOperation" method dynamically.
                var genericType =
                    objectType.ImplementsGenericInterface(typeof(IOperation<>)) ? objectType
                        .GetGenericInterface(typeof(IOperation<>))
                        .GetGenericArguments()
                        [0] :
                    objectType.HasGenericInterfaceDefinition(typeof(IOperation<>)) ? objectType
                        .GetGenericArguments()
                        [0] :
                    throw new ArgumentException($"invalid object type: {objectType}");

                var readOperationMethod = ReadOperationMethod.MakeGenericMethod(genericType);

                return this.InvokeFunc(
                    readOperationMethod,
                    operationToken,
                    serializer);
                    //JsonSerializer.Create(Constants.JsonSettings));
            }            
        }

        private IOperation<TResult> ReadOperation<TResult>(JObject operationToken, JsonSerializer serializer)
        {
            var isSucceeded = operationToken.Value<bool>(nameof(IOperation.Succeeded));
            if (isSucceeded)
            {
                if (operationToken.TryGetValue(ResultJsonPropertyName, out var result))
                    return Operation.FromResult(result.ToObject<TResult>(serializer));

                else
                    return Operation.FromResult<TResult>(default);
            }
            else
            {
                var errorProp = nameof(IOperation.Error);
                if (operationToken.TryGetValue(errorProp, out var error))
                    return error != null
                        ? Operation.Fail<TResult>(error.ToObject<OperationError>(serializer)) //JsonSerializer.Create(Constants.JsonSettings)))
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

            else if(value.GetType().Equals(typeof(IOperation)) || value.GetType().Implements(typeof(IOperation)))
            {
                OperationJsonConverter
                    .ToJToken((IOperation)value, serializer)
                    .WriteTo(writer);
            }

            else
            {
                var objectType = value.GetType();
                var genericType =
                    objectType.ImplementsGenericInterface(typeof(IOperation<>)) ? objectType
                        .GetGenericInterface(typeof(IOperation<>))
                        .GetGenericArguments()
                        [0] :
                    objectType.HasGenericInterfaceDefinition(typeof(IOperation<>)) ? objectType
                        .GetGenericArguments()
                        [0] :
                    throw new ArgumentException($"invalid object type: {objectType}");

                var writeOperationMethod = WriteOperationMethod.MakeGenericMethod(genericType);
                this.InvokeAction(
                    writeOperationMethod,
                    writer,
                    value,
                    serializer);
            }
            
        }

        private void WriteOperation<TResult>(JsonWriter writer, IOperation<TResult> operation, JsonSerializer serializer)
        {
            OperationJsonConverter
                .ToJToken(operation, serializer)
                .WriteTo(writer);
        }

        public static JToken ToJToken(IOperation operation, JsonSerializer serializer)
        {
            if (operation == null)
                return JValue.CreateNull();

            if (operation.Succeeded == null)
                throw new InvalidOperationException("Cannot serialize an unresolved operation");

            JObject joperation = new JObject
            {
                [nameof(IOperation.Succeeded)] = operation.Succeeded ?? throw new InvalidOperationException("Cannot serialize an unresolved operation")
            };

            if (operation.Error != null)
                joperation[nameof(IOperation.Error)] = OperationErrorJsonConverter.ToJToken(operation.Error, serializer);

            return joperation;
        }

        public static JToken ToJToken<TResult>(IOperation<TResult> operation, JsonSerializer serializer)
        {
            if (operation == null)
                return JValue.CreateNull();

            if (operation.Succeeded == null)
                throw new InvalidOperationException("Cannot serialize an unresolved operation");

            JObject joperation = new JObject
            {
                [nameof(IOperation.Succeeded)] = operation.Succeeded
            };

            if (operation.Succeeded == true)
            {
                TResult result = operation switch
                {
                    IResolvable<TResult> valueResolvable => valueResolvable.Resolve(),

                    IResolvable<Task<TResult>> taskResolvable => taskResolvable.Resolve().Result, //safe because operation.Succeed == true

                    _ => throw new ArgumentException($"Invalid operation type: {operation.GetType()}")
                };

                joperation[ResultJsonPropertyName] = result != null
                    ? JObject.FromObject(result, serializer).As<JToken>()
                    : JValue.CreateNull();
            }
            else 
                joperation[nameof(IOperation.Error)] = OperationErrorJsonConverter.ToJToken(operation.Error, serializer);

            return joperation;
        }

    }
}
