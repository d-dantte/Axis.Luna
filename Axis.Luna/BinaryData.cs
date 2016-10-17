using System;
using System.IO;
using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    /// <summary>
    /// Represents data than may be embeded locally in this object, or may be referenced from some external source via a uri (file system uri, url, etc)
    /// </summary>
    public class BinaryData
    {
        public string Data { get; set; }
        public bool IsDataEmbeded { get; set; }
        public string Name { get; set; }
        public string Mime => MimeObject().MimeCode;


        public string Extension() => Eval(() => Name.Substring(Name.LastIndexOf('.'))); //index should include the '.'
        public Mime MimeObject() => MimeMap.ToMimeObject(Extension());

        public byte[] EmbededByteStream() => IsDataEmbeded ? Convert.FromBase64String(Data) : null;
        public Stream EmbededDataStream() => IsDataEmbeded ? new MemoryStream(EmbededByteStream()) : null;
        public string EmbededDataURI() => IsDataEmbeded ? $"data:{MimeObject().MimeCode};base64,{Data}" : null;

        public BinaryData EmbedData(string name, byte[] data)
        {
            IsDataEmbeded = true;
            Data = Convert.ToBase64String(data);
            Name = name;
            return this;
        }
        public BinaryData EmbedData(string dataUri)
        {
            var parts = dataUri.TrimStart("data:").Split(';');
            Name = $"data.{parts[0].ToExtension()}";
            Data = parts[1].TrimStart("base64,");
            IsDataEmbeded = true;
            return this;
        }
        public BinaryData ReferenceData(string locationUri, string name = null)
        {
            IsDataEmbeded = false;
            Name = name;
            Data = locationUri;
            return this;
        }

        #region init
        public BinaryData()
        { }
        #endregion
    }
}
