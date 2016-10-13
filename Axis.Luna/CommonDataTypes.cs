namespace Axis.Luna
{

    public enum CommonDataType
    {
        String,
        Integer,
        Real,
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

        UnknownType
    }
}
