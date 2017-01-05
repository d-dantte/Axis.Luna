using System;
using System.IO;

using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    public class EncodedBinaryData
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public string Mime => MimeObject().MimeCode;

        public string Extension() => Eval(() => Name.Substring(Name.LastIndexOf('.'))); //index should include the '.'
        public Mime MimeObject() => MimeMap.ToMimeObject(Extension());

        public byte[] ByteArray() => Convert.FromBase64String(Data);
        public Stream ByteStream() => new MemoryStream(ByteArray());
        public string DataUri() => $"data:{Mime};base64,{Data}";
    }
}
