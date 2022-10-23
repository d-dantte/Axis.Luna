namespace Axis.Luna.Common.Types.Basic
{
    public static class BasicExtensions
    {
        public static BasicValueWrapper Wrap(this IBasicValue value) => new BasicValueWrapper(value);
    }
}
