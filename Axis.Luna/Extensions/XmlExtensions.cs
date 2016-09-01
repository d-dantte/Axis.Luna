using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Axis.Luna.Extensions
{
    public static class XmlExtensions
    {
        public static IEnumerable<XElement> Elements(this XElement @this, Func<XElement, bool> predicate)
            => @this.Elements().Where(predicate);

        public static IEnumerable<XElement> ElementsWithLocalName(this XElement @this, string localName)
            => @this.Elements(elt => elt.Name.LocalName == localName);

        public static XElement ElementWithLocalName(this XElement @this, string localName)
            => @this.ElementsWithLocalName(localName).FirstOrDefault();


        public static IEnumerable<XAttribute> Attributes(this XElement @this, Func<XAttribute, bool> predicate)
            => @this.Attributes().Where(predicate);

        public static IEnumerable<XAttribute> AttributesWithLocalName(this XElement @this, string localName)
            => @this.Attributes(elt => elt.Name.LocalName == localName);

        public static XAttribute AttributeWithLocalName(this XElement @this, string localName)
            => @this.AttributesWithLocalName(localName).FirstOrDefault();
    }
}
