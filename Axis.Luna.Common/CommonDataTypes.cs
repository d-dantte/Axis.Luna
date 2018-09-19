using System;
using System.Collections.Generic;
using System.Text;

namespace Axis.Luna.Common
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

        DateTime,
        TimeSpan,

        Url,
        IPV4,
        IPV6,
        Phone,
        Email,
        Location,
        Guid,

        UnknownType,

        //others
        Tags,
        CSV
    }
}
