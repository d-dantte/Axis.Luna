using System.Collections.Generic;

namespace Axis.Luna
{
    public class BlobRef
    {
        private TagBuilder _metadata = TagBuilder.Create();

        public string Uri { get; set; }

        /// <summary>
        /// Case insensitive, css-formatted list of name-value pairs
        /// </summary>
        public string Metadata
        {
            get { return _metadata.ToString(); }
            set { _metadata = TagBuilder.Create(value); }
        }

        public IEnumerable<Tag> MetadataTags() => TagBuilder.Parse(Metadata);
    }
}
