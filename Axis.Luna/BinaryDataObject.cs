using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna
{
    public interface IBinaryDataObject
    {
        string Name { get; set; }
        string Address { get; set; }
        string Mime { get; set; }
        byte[] Data { get; set; }

        string Extension();
        Stream DataStream();
    }
}
