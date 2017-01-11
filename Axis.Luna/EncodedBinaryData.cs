using System;
using System.Collections.Generic;
using System.IO;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    public class EncodedBinaryData
    {
        private string _mime = null;

        public string Data { get; set; }
        public string Name { get; set; }
        public string Metadata { get; set; }
        public string Mime
        {
            get { return _mime ?? MimeObject().MimeCode; }
            set { _mime = value?.Trim(); }
        }

        public string Extension() 
            => Eval(() => Name.Substring(Name.LastIndexOf('.')))?.Trim(); //index should include the '.'

        public Mime MimeObject()
            => string.IsNullOrWhiteSpace(_mime) ?
               MimeMap.ToMimeObject(Extension()) :
               new Mime { MimeCode = _mime, Extension = Extension() ?? "." };

        public byte[] ByteArray() => Convert.FromBase64String(Data);
        public Stream ByteStream() => new MemoryStream(ByteArray());
        public string DataUri() => $"data:{Mime};base64,{Data}";

        public IEnumerable<Tag> MetadataTags() => TagBuilder.Parse(Metadata);

        #region Init
        public EncodedBinaryData()
        { }

        public EncodedBinaryData(byte[] data, string name, string mime = null, string metadata = null)
        {
            Data = Convert.ToBase64String(data);
            Name = name;
            Mime = mime;
            Metadata = metadata;
        }

        public EncodedBinaryData(Stream data, string name, string mime = null, string metadata = null)
        : this(new MemoryStream().UsingValue(_ms => data.CopyTo(_ms)).ToArray(), name, mime, metadata)
        { }
        #endregion
    }
}
