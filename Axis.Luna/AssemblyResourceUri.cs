using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.ExceptionExtensions;
using static Axis.Luna.Extensions.EnumerableExtensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Axis.Luna
{
    public class AssemblyResourceUri : Uri
    {
        static AssemblyResourceUri()
        {
            UriParser.Register(new ARUParser(), "rx", -1);
            UriParser.Register(new ARUParser(), "arx", -1);
        }

        /// <summary>
        /// Creates a new Assembly resource uri on the form 
        /// "rx|arx://{assembly-short-name}/{path-to-resource-file}#{ResouceName}"
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static AssemblyResourceUri NewUri(string uri, string defaultAssembly = null)
            => defaultAssembly.ValidateAssemblyName()
                              .Pipe(da => ARUParser.SchemeFormat.IsMatch(uri) ?
                                             uri : $"arx://{defaultAssembly.ThrowIfNull()}/{uri.TrimStart("/")}")
                              .Pipe(auri => new AssemblyResourceUri(auri));


        private AssemblyResourceUri(string uri) : base(uri)
        {
            IsCompact = ARUParser.HostFormat
                                 .Match(uri)
                                 .Pipe(m => m.Success && m.Value.Contains(";/"));
        }

        public bool IsCompact { get; private set; }

        public string Assembly => Authority;
        public string ManifestResourcePath => AbsolutePath.TrimStart("/").Replace("/", ".");

    }

    public class ARUParser : GenericUriParser
    {
        public static readonly Regex AbsoluteFormat = new Regex(@"^(rx|arx)\:(//\w[\w\.]*\;?(?=/))?(((?<!/)(/[^/#]+)+)|((?<=\:)([^/#]+)?(/[^/#]+)+))(#[\w\.]+)?$",
                                                                RegexOptions.IgnoreCase);
        public static readonly Regex SchemeFormat = new Regex(@"^(rx|arx)\:", RegexOptions.IgnoreCase);
        public static readonly Regex HostFormat = new Regex(@"//\w[\w\.]*\;?(?=/)");
        public static readonly Regex SegmentFormat = new Regex(@"(?<!/)/[^/#]+");
        public static readonly Regex PathFormat = new Regex(@"((?<!/)(/[^/#]+)+)|((?<=\:)([^/#]+)?(/[^/#]+)+)");
        public static readonly Regex FragmentFormat = new Regex(@"#[\w\.]+$");
        public static readonly Regex AssemblyPattern = new Regex(@"[\w\.]+");


        public ARUParser()
        : base(GenericUriParserOptions.AllowEmptyAuthority
             | GenericUriParserOptions.NoPort
             | GenericUriParserOptions.NoQuery
             | GenericUriParserOptions.NoUserInfo)
        { }

        protected override void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
        {
            if (!AbsoluteFormat.IsMatch(uri.OriginalString)) parsingError = new UriFormatException();
            else parsingError = null;
        }

        public static new bool Equals(Object objA, Object objB)
        {
            var uria = objA as Uri;
            var urib = objB as Uri;

            return uria != null && urib != null &&
                   uria.Scheme == urib.Scheme &&
                   uria.Segments.SequenceEqual(urib.Segments) &&
                   uria.Fragment == urib.Fragment;
        }

        protected override bool IsWellFormedOriginalString(Uri uri) => true;

        protected override UriParser OnNewUri() => new ARUParser();

        protected override string GetComponents(Uri uri, UriComponents components, UriFormat format)
        {
            var uris = uri.OriginalString;
            switch (components)
            {
                case UriComponents.AbsoluteUri:
                {
                    var scheme = SchemeFormat.Match(uris);
                    var host = HostFormat.Match(uris);
                    var path = PathFormat.Match(uris);
                    var frag = FragmentFormat.Match(uris);

                    if (scheme.Success && path.Success)
                    {
                        return scheme.Value
                             + host.Value //the host is optional and will be empty if absent
                             + path.Value
                             + frag.Value; //frag is also optional and will be empty
                    }
                    else return "";
                }
                case UriComponents.Path:
                case UriComponents.PathAndQuery:
                case UriComponents.Path | UriComponents.KeepDelimiter:
                case UriComponents.Path | UriComponents.Query | UriComponents.KeepDelimiter:
                {
                    var host = GetComponents(uri, UriComponents.Host | UriComponents.KeepDelimiter, format);
                    var isCompact = host.Contains(";");
                    host = host.Replace("/", "").Replace(";", "");
                    var path = PathFormat.Match(uris);
                    if (path.Success)
                    {
                        var sb = new StringBuilder();
                        if (isCompact)
                            sb.Append(path.Value.StartsWith("/") ? "/" : "").Append(host).Append(path.Value.StartsWith("/") ? "" : "/");
                        sb.Append(path.Value);
                        return sb.ToString();
                    }
                    else return "";
                }
                case UriComponents.Host:
                case UriComponents.HostAndPort:
                case UriComponents.Host | UriComponents.Port:
                case UriComponents.Host | UriComponents.KeepDelimiter:
                {
                    var host = HostFormat.Match(uris);
                    if (host.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) return host.Value;
                        else return host.Value.Replace("/", "").Replace(";", "");
                    }
                    else return "";
                    //{
                    //    var aruri = uri as AssemblyResourceUri;
                    //    if (aruri != null && !string.IsNullOrWhiteSpace(aruri.DefaultAssembly)) return aruri.DefaultAssembly;
                    //    else return Assembly.GetExecutingAssembly().GetName().Name;
                    //}
                }
                case UriComponents.Fragment:
                case UriComponents.Fragment | UriComponents.KeepDelimiter:
                {
                    var frag = FragmentFormat.Match(uris);
                    if (frag.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) return frag.Value;
                        else return frag.Value.Replace("#", "");
                    }
                    else return "";
                }
                case UriComponents.NormalizedHost:
                {
                    var host = GetComponents(uri, UriComponents.Host, format);
                    return host.ToLower();
                }
                case UriComponents.Scheme:
                case UriComponents.Scheme | UriComponents.KeepDelimiter:
                {
                    var scheme = SchemeFormat.Match(uris);
                    if (scheme.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) return scheme.Value;
                        else return scheme.Value.Replace(":", "");
                    }
                    else return "";
                }
                default: return "";
            }
        }
    }

    public static class AssemblyResourceUriExtensions
    {
        public static Stream ToResourceStream(this AssemblyResourceUri uri)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(ass => ass.GetName().Name == uri.Host);
            if (assembly == null) //load the assembly
                assembly = Assembly.Load(new AssemblyName { Name = uri.Host });

            return assembly.GetManifestResourceStream(uri.ManifestResourcePath);
        }

        internal static string ValidateAssemblyName(this string defaultAssembly)
        {
            if (defaultAssembly == null) return null;

            else if (string.Empty.Equals(defaultAssembly.Trim()))
                throw new ArgumentException(nameof(defaultAssembly));

            else if (!ARUParser.AssemblyPattern.Match(defaultAssembly).Success)
                throw new ArgumentException(nameof(defaultAssembly));

            else return defaultAssembly;
        }
    }
}
