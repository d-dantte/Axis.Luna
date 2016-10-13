using System;
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
        public string DataUrl() => $"data:{MimeObject().MimeCode};base64,{Convert.ToBase64String(Data)}";

        #region init
        public BinaryData()
        { }

        public BinaryData(string dataUrl)
        {
            var parts = dataUrl.TrimStart("data:").Split(';');
            Mime = parts[0];
            Data = parts[1].TrimStart("base64,").Pipe(b64 => Convert.FromBase64String(b64));
        }
        #endregion
    }
}
