using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Axis.Luna.Extensions;

namespace Axis.Luna.Common.Utils
{

    /// <summary>
    /// 
    /// </summary>
    public class AssemblyResourceUri : Uri
    {
        static AssemblyResourceUri()
        {
            UriParser.Register(new ARUParser(), "rx", -1);
            UriParser.Register(new ARUParser(), "arx", -1);
        }

        /// <summary>
        /// Creates a new Assembly resource uri of the form 
        /// "rx|arx://{assembly-short-name}/{path-to-resource-file}#{ResouceName}"
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static AssemblyResourceUri NewAbsoluteUri(string uri) => new AssemblyResourceUri(uri);

        /// <summary>
        /// Creates a new Assembly resource uri of the form 
        /// "rx|arx://{assembly-short-name}/{path-to-resource-file}#{ResouceName}"
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="targetAssembly"></param>
        /// <returns></returns>
        public static AssemblyResourceUri NewRelativeUri(string uri, string targetAssembly = null)
        {
            var assemblyName = targetAssembly ?? System.Reflection.Assembly.GetCallingAssembly().GetName().Name;
            uri = uri.Trim();
            var isCompactRelative = uri.StartsWith(";/");
            if (!isCompactRelative && !uri.StartsWith("/")) 
                throw new Exception("invalid uri");

            var asn = assemblyName.ValidateAssemblyName();
            var _uri = $"arx://{asn}{uri}";
            return new AssemblyResourceUri(_uri);
        }


        private AssemblyResourceUri(string uri) : base(uri)
        {
            var matcher = ARUParser.HostFormat.Match(uri);
            IsCompact = matcher.Success && matcher.Value.Contains(";/");
        }

        public bool IsCompact { get; private set; }

        public string Assembly => Authority;

        public string ManifestResourcePath => _manifestResourcePath ??= AbsolutePath.TrimStart("/").Replace("/", ".");

        private string _manifestResourcePath = null;

    }

    public class ARUParser : GenericUriParser
    {
        public static readonly Regex AbsoluteFormat = new Regex(
            @"^(rx|arx)\:(//\w[\w\.]*\;?(?=/))?(((?<!/)(/[^/#]+)+)|((?<=\:)([^/#]+)?(/[^/#]+)+))(#[\w\.]+)?$",
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
            if (!AbsoluteFormat.IsMatch(uri.OriginalString)) 
                parsingError = new UriFormatException();

            else parsingError = null;
        }

        public static new bool Equals(Object objA, Object objB)
        {
            return objA is Uri uria
                && objB is Uri urib
                && urib != null 
                && uria.Scheme == urib.Scheme
                && uria.Segments.SequenceEqual(urib.Segments)
                && uria.Fragment == urib.Fragment;
        }

        protected override bool IsWellFormedOriginalString(Uri uri) => true;

        protected override UriParser OnNewUri() => new ARUParser();

        protected override string GetComponents(Uri uri, UriComponents components, UriFormat format)
        {
            var uris = uri.OriginalString;
            switch (components)
            {
                case UriComponents.AbsoluteUri:
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

                case UriComponents.Path:
                case UriComponents.PathAndQuery:
                case UriComponents.Path | UriComponents.KeepDelimiter:
                case UriComponents.Path | UriComponents.Query | UriComponents.KeepDelimiter:
                    var hostString = GetComponents(uri, UriComponents.Host | UriComponents.KeepDelimiter, format);
                    var isCompact = hostString.Contains(";");
                    hostString = hostString.Replace("/", "").Replace(";", "");
                    var pathMatch = PathFormat.Match(uris);
                    if (pathMatch.Success)
                    {
                        var sbuilder = new StringBuilder();
                        if (isCompact)
                            sbuilder
                                .Append(pathMatch.Value.StartsWith("/") ? "/" : "")
                                .Append(hostString)
                                .Append(pathMatch.Value.StartsWith("/") ? "" : "/");

                        sbuilder.Append(pathMatch.Value);
                        return sbuilder.ToString();
                    }
                    else return "";

                case UriComponents.Host:
                case UriComponents.HostAndPort:
                case UriComponents.Host | UriComponents.Port:
                case UriComponents.Host | UriComponents.KeepDelimiter:
                    var hostMatch = HostFormat.Match(uris);
                    if (hostMatch.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) 
                            return hostMatch.Value;

                        else 
                            return hostMatch.Value.Replace("/", "").Replace(";", "");
                    }
                    else return "";

                case UriComponents.Fragment:
                case UriComponents.Fragment | UriComponents.KeepDelimiter:
                    var fragMatch = FragmentFormat.Match(uris);
                    if (fragMatch.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) 
                            return fragMatch.Value;

                        else 
                            return fragMatch.Value.Replace("#", "");
                    }
                    else return "";

                case UriComponents.NormalizedHost:
                    hostString = GetComponents(uri, UriComponents.Host, format);
                    return hostString.ToLower();

                case UriComponents.Scheme:
                case UriComponents.Scheme | UriComponents.KeepDelimiter:
                    var schemeMatch = SchemeFormat.Match(uris);
                    if (schemeMatch.Success)
                    {
                        if (components.HasFlag(UriComponents.KeepDelimiter)) 
                            return schemeMatch.Value;

                        else 
                            return schemeMatch.Value.Replace(":", "");
                    }
                    else return "";

                default: return "";
            }
        }
    }

    public static class AssemblyResourceUriExtensions
    {
        public static System.IO.Stream ToResourceStream(this AssemblyResourceUri uri)
        {
            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(ass => ass.GetName().Name == uri.Host);

            if (assembly == null) //load the assembly
                assembly = Assembly.Load(new AssemblyName { Name = uri.Host });

            return assembly.GetManifestResourceStream(uri.ManifestResourcePath);
        }

        internal static string ValidateAssemblyName(this string defaultAssembly)
        {
            if (defaultAssembly == null) throw new Exception("null assembly root");

            else if (string.Empty.Equals(defaultAssembly.Trim()))
                throw new ArgumentException(nameof(defaultAssembly));

            else if (!ARUParser.AssemblyPattern.Match(defaultAssembly).Success)
                throw new ArgumentException(nameof(defaultAssembly));

            else return defaultAssembly;
        }
    }
}
