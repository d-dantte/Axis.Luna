namespace Axis.Luna.Utils
{

    public enum CommonDataType
    {
        String,
        Integer,
        Real,
        Decimal, //different from real because it holds exact figures, whereas reals hold approximations
        Boolean,
        Binary,

        JsonObject,

        /// <summary>
        /// Json encoded representation of a Axis.Luna.BinaryData object
        /// </summary>
        BinaryData,

        DateTime,
        TimeSpan,

        Url,
        IPV4,
        IPV6,
        Phone,
        Email,
        Location,

        UnknownType,

        //others
        Tags
    }
}
