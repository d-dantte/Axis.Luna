using Axis.Luna.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using static Axis.Luna.Extensions.ObjectExtensions;

namespace Axis.Luna
{
    /// <summary>
    /// <para>
    /// The TagAttribute is a general purpose attribute used to decorate any c# construct, attaching a custom name/value pair list to it.
    /// At runtime, one can then inspect the class metadata for the tag attribute and extract the name value list from it. 
    /// </para>
    /// <para>
    /// The name/value pair list is encoded within the attribute in a css-like manner. Any non-empty string not containing ":" or ";"
    /// may be used as a name, and any string may be used for it's value - ':', ';' and '@' are delimited within the string to 
    /// enable proper parsing of the string. Encoding is done by substitutions as follows:
    /// <para>
    /// <list type="bullet">
    ///     <item><description>substitute ':' with '@col'</description></item>
    ///     <item><description>substitute ';' with '@scol'</description></item>
    ///     <item><description>substitute '@' with '@at'</description></item>
    /// </list>
    /// </para>
    /// Usage of this attribute is as follows:
    /// <para>
    /// <code>
    /// [Tags("name:value; some-other-name:value for this tag; yet another:value with @scol delimited; emal:stanley@atyahoo.com")]
    /// public class MyTaggedClass
    /// {
    /// }
    /// </code>
    /// </para>
    /// </para>
    /// </summary>
    [AttributeUsage(System.AttributeTargets.All)]
    public class TagsAttribute : System.Attribute
    {
        private Tag[] _tags = null;

        public IEnumerable<Tag> Tags => _tags ?? new Tag[0];

        public Tag this[string tname] => _tags?.FirstOrDefault(t => t.Name == tname);

        public TagsAttribute(string tagPairs)
        {
            initTags(tagPairs);
        }
        public TagsAttribute(TagBuilder builder)
            : this(builder.ToString())
        { }

        private void initTags(string tpairs)
        {
            this._tags = TagBuilder.Parse(tpairs);
        }
    }

    public class TagBuilder
    {
        private Dictionary<string,Tag> tags = new Dictionary<string, Tag>();
        public IEnumerable<Tag> Tags => tags.Select(_kvp => _kvp.Value).ToArray();


        public static TagBuilder Create() => new TagBuilder();

        public static TagBuilder Create(string tags) 
        => Parse(tags).Aggregate(new TagBuilder(), (builder, tag) => builder.Add(tag.Name, tag.Value));

        public static TagBuilder Create(IEnumerable<KeyValuePair<string, string>> pairs)
        => pairs.Aggregate(new TagBuilder(), (builder, pair) => builder.Add(pair.Key, pair.Value));

        public bool ContainsTag(string name) => tags.ContainsKey(name);

        public TagBuilder Add(string tname, string tvalue)
        {
            tags.Add(tname, new Tag(tname, tvalue));
            return this;
        }
        public TagBuilder Remove(string name)
        {
            tags.Remove(name);
            return this;
        }

        public Tag GetOrAdd(string name, string value)
            => tags.GetOrAdd(name, _k => new Tag(name, value));


        public override string ToString()
        => tags.Aggregate(new StringBuilder(),
                          (sb, next) => sb.Append(sb.Length == 0 ? "" : " ")
                                          .Append(TagCodec.Encode(next.Key)).Append(":")
                                          .Append(TagCodec.Encode(next.Value.Value)).Append(";"))
               .ToString();


        public static Tag[] Parse(string tpairs)
        {
            if (string.IsNullOrWhiteSpace(tpairs)) return new Tag[0];
            //else

            //get all tag pairs
            var tagPairs = TagCodec.AltEncode(tpairs).Split(';');
            var tempList = new List<Tag>();

            //get individual tags
            for (int cnt = 0; cnt < tagPairs.Length; cnt++)
            {
                if (string.IsNullOrWhiteSpace(tagPairs[cnt])) continue;

                var tag = tagPairs[cnt].Split(':');
                if (string.IsNullOrWhiteSpace(tag[0])) continue;

                tempList.Add(new Tag(TagCodec.AltDecode(tag[0]),
                                     TagCodec.AltDecode(tag[1])));
            }

            return tempList.ToArray();
        }
    }

    public class Tag
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public Tag(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("invalid tag name");

            this.Name = ("" + name).Trim();
            this.Value = ("" + value).Trim();
        }

        public Tag Clone() => new Tag(Name, Value);

        public override string ToString() => $"{Name}:{Value};";
        //public override string ToString() => $"{TagCodec.Encode(Name)}:{TagCodec.Encode(Value)};";

        public override bool Equals(object obj)
            => obj.Cast<Tag>().Pipe(_t => _t?.Name == Name && _t?.Value == Value);

        public override int GetHashCode() => this.PropertyHash();
    }

    public static class TagCodec
    {
        public static string ALT = "!";

        private static Dictionary<string, TagDelimiter> _Delimiters = new Dictionary<string, TagDelimiter>
        {
            //{"col", new TagDelimiter{ code="&col;", value=":"}},
            //{"amp", new TagDelimiter{ code="&amp;", value="&"}},
            //{"scol", new TagDelimiter{ code="&scol;", value=";"}}
            {"col", new TagDelimiter{ Code="@col", Value=":"}},
            {"at", new TagDelimiter{ Code="@at", Value="@"}},
            {"scol", new TagDelimiter{ Code="@scol", Value=";"}}
        };

        /// <summary>
        /// Decode an <code>TagCodec.AltEncode</code> encoded string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AltDecode(string value)
        {
            var aux = value;
            if (string.IsNullOrWhiteSpace(value)) return value;
            else foreach (var td in _Delimiters.Values) aux = aux.Replace(AltCode(td), td.Value);
            return aux;
        }
        public static string Decode(string value)
        {
            var aux = value;
            if (string.IsNullOrWhiteSpace(value)) return value;
            else foreach (var td in _Delimiters.Values) aux = aux.Replace(td.Code, td.Value);
            return aux;
        }

        /// <summary>
        /// Encode an already encoded string using the alternate codes: i.e replace &col; with &col!
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AltEncode(string value)
        {
            var aux = value;
            if (string.IsNullOrWhiteSpace(value)) return value;
            else foreach (var td in _Delimiters.Values) aux = aux.Replace(td.Code, AltCode(td));
            return aux;
        }
        public static string Encode(string value)
        {
            var aux = value;
            if (string.IsNullOrWhiteSpace(value)) return value;
            else foreach (var td in _Delimiters.Values) aux = aux.Replace(td.Value, td.Code);
            return aux;
        }

        private static string AltCode(TagDelimiter td)
        {
            if (td == null || string.IsNullOrWhiteSpace(td.Code)) return "";
            return td.Code.Replace(';', ALT[0]);
        }
        private static string ReCode(TagDelimiter td)
        {
            if (td == null || string.IsNullOrWhiteSpace(td.Code)) return "";
            return td.Code.Replace(ALT[0], ';');
        }

        public static TagsAttribute Tags(this MemberInfo member, bool ancestorlookup = false)
        {
            if (!_tagCache.ContainsKey(member))
            {
                var t = member.GetCustomAttribute<TagsAttribute>(ancestorlookup);
                //if (t == null) _tagCache[member] = new TagAttributeHolder { tagAttribute = null };
                //else _tagCache[member] = new TagAttributeHolder { tagAttribute = t };
                // or simply
                _tagCache[member] = new TagAttributeHolder { TagAttribute = t };
            }

            return _tagCache[member].TagAttribute;
        }
        public static void FlushCache(this MemberInfo member)
        {
            if (_tagCache.ContainsKey(member)) _tagCache.Remove(member);
        }
        public static void FlushAll()
        {
            _tagCache.Clear();
        }
        private static Dictionary<MemberInfo, TagAttributeHolder> _tagCache = new Dictionary<MemberInfo, TagAttributeHolder>();
    }


    internal class TagAttributeHolder
    {
        internal TagsAttribute TagAttribute { get; set; }
    }


    internal class TagDelimiter
    {
        public string Value { get; set; }
        public string Code { get; set; }
    }
}
