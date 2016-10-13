using System.IO;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    public class BinaryData
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public string Mime { get; set; }

        public byte[] Data { get; set; }

        public string Extension() => Eval(() => Name.Substring(Name.LastIndexOf('.') + 1));
        public Stream DataStream() => new MemoryStream(Data);
        public Mime MimeObject() => MimeMap.ToMimeObject(Extension());
    }
}
