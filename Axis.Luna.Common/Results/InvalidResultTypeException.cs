using System;

namespace Axis.Luna.Common.Results
{
    /// <summary>
    /// Indicates that the encountered result instance is not one of either <see cref="IResult{TData}.DataResult"/>, or <see cref="IResult{TData}.ErrorResult"/>
    /// </summary>
    public class InvalidResultTypeException : Exception
    {
        /// <summary>
        /// The invalid type that caused this exception
        /// </summary>
        public Type ResultType { get; }

        public InvalidResultTypeException(Type invalidType)
            : base($"The supplied result is not a valid {typeof(IResult<>)} implementation: '{invalidType}'")
        {
            ResultType = invalidType ?? throw new ArgumentNullException(nameof(invalidType));
        }
    }
}
