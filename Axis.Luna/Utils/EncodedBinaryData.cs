using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna.Utils
{
    public class EncodedBinaryData
    {
        private string _mime = null;
        private TagBuilder _metadata = TagBuilder.Create();

        public byte[] Data { get; set; }

        /// <summary>
        /// Case insensitive, css-formatted list of name-value pairs
        /// </summary>
        public string Metadata
        {
            get { return _metadata.ToString(); }
            set
            {
                var name = Name;
                _metadata = TagBuilder.Create(value);
                if (!string.IsNullOrWhiteSpace(name)) _metadata.Add("name", name);
            }
        }
        public string Name
        {
            get
            {
                return _metadata.Tags.FirstOrDefault(_t => _t.Name.ToLower() == "name")?.Value;
            }
            set { _metadata.Remove("name").Add("name", value); }
        }
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

        public string Base64() => Convert.ToBase64String(Data);
        public Stream ByteStream() => new MemoryStream(Data);
        public string DataUri() => $"data:{Mime};base64,{Base64()}";

        public IEnumerable<Tag> MetadataTags() => TagBuilder.Parse(Metadata);

        #region Init
        public EncodedBinaryData()
        { }

        public EncodedBinaryData(byte[] data, string mime, string name = null, string metadata = null)
        {
            Data = data;
            Mime = mime;
            Name = name;
            Metadata = metadata;
        }

        public EncodedBinaryData(Stream data, string mime, string name = null, string metadata = null)
        : this(new MemoryStream().UsingValue(_ms => data.CopyTo(_ms)).ToArray(), mime, name, metadata)
        { }
        #endregion
    }
}
