using GridLuck.Common;
using GridLuck.Common.Extensions;
using GridLuck.Common.Results;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GridLuck.Infra.AsynOps
{
    public enum AsyncOpType
    {
        Command,
        Stream,
        Event
    }

    /// <summary>
    /// urn:asyncop:{cmd|seq|evt}:{namespace}:{guid}
    /// </summary>
    public readonly struct OperationId : IDefaultValueProvider<OperationId>
    {
        /// <summary>
        /// Namespace ID
        /// </summary>
        public const string NID = "asyncop";

        public const string Scheme = "urn";

        public static readonly Regex Pattern = new(
            @"^urn:asyncop:(?<operation>cmd|seq|evt):(?<domain>\*|([a-zA-Z_]+[a-zA-Z_\.-]*)):(?<guid>[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{2}-[a-fA-F0-9]{2}(-[a-fA-F0-9]{4}){4}-[a-fA-F0-9]{8})$",
            RegexOptions.Compiled);

        public OperationNamespace Namespace { get; }

        public Suid UUId { get; }

        public AsyncOpType OperationType { get; }

        public static OperationId Default => default;

        public bool IsDefault => Namespace.IsDefault && UUId.IsDefault && OperationType == default;

        public OperationId(AsyncOpType type, OperationNamespace @namespace, Suid uuid)
        {
            Namespace = @namespace;
            UUId = uuid;
            OperationType = type.ThrowIf(
                t => !Enum.IsDefined(t),
                _ => new ArgumentOutOfRangeException(nameof(type), $"Invalid type: {type}]"));
        }

        /// <summary>
        /// Creates an Id from the given params, and creates a new random <see cref="Suid"/> instance
        /// </summary>
        /// <param name="type">The op-type</param>
        /// <param name="namespace">The namespace</param>
        public OperationId(
            AsyncOpType type,
            OperationNamespace @namespace)
            : this(type, @namespace, Suid.Create())
        {
        }

        public OperationId(AsyncOpType type)
            : this(type, default)
        {
        }

        public static OperationId Of(AsyncOpType type, string @namespace, Suid uuid) => new(type, @namespace, uuid);

        public static OperationId Of(string identifier) => Parse(identifier);

        public static implicit operator OperationId((AsyncOpType type, string domain, Suid id) info) => new(info.type, info.domain, info.id);

        public static implicit operator OperationId(string identifier) => Parse(identifier);

        #region Parse
        public static OperationId Parse(string identifier)
        {
            _ = !TryParse(identifier, out var result);
            return result.Resolve();
        }

        public static bool TryParse(string identifierText, out Result<OperationId> identifierResult)
        {
            if (string.IsNullOrWhiteSpace(identifierText))
            {
                identifierResult = default(OperationId);
                return false;
            }

            var parts = identifierText.Split(':');

            if (parts.Length == 5
                && Scheme.Equals(parts[0]) // Scheme
                && NID.Equals(parts[1]) // NID
                && TryParseOpType(parts[2], out var optype) // Operation Type
                && OperationNamespace.TryParse(parts[3], out var ns) // Namespace
                && Suid.TryParse(parts[4], out var uuid))
            {
                identifierResult = new OperationId(optype, ns.Resolve(), uuid.Resolve());
                return true;
            }

            identifierResult = new FormatException($"Invalid format: {identifierText}");
            return false;
        }

        private static string ToText(AsyncOpType type)
        {
            return type switch
            {
                AsyncOpType.Command => "cmd",
                AsyncOpType.Stream => "seq",
                AsyncOpType.Event => "evt",
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
        }

        private static bool TryParseOpType(string text, out AsyncOpType type)
        {
            type = text switch
            {
                "cmd" => AsyncOpType.Command,
                "evt" => AsyncOpType.Event,
                "seq" => AsyncOpType.Stream,
                _ => (AsyncOpType)(-1)
            };

            return Enum.IsDefined(type);
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"{Scheme}:{NID}:{ToText(OperationType)}:{Namespace}:{UUId}";
        }
        #endregion
    }

    public readonly struct OperationNamespace :
        IDefaultValueProvider<OperationNamespace>,
        IEquatable<OperationNamespace>
    {
        public static readonly Regex Pattern = new(@"^\*|([a-zA-Z_]+[a-zA-Z_\.-]*)$", RegexOptions.Compiled);

        private readonly string? _namespace;

        public static OperationNamespace Default => default;

        public bool IsDefault => _namespace is null;

        public OperationNamespace(string @namespace)
        {
            _namespace = @namespace
                .ThrowIf(
                    string.IsNullOrWhiteSpace,
                    _ => new ArgumentException($"Invalid {nameof(@namespace)}: null/empty/whitespace"))
                .ThrowIfNot(
                    Pattern.IsMatch,
                    text => new FormatException($"Invalid format: {@namespace}"))
                .ApplyTo(text => "*".Equals(text) ? null : text);
        }

        #region overrides
        public override string ToString()
        {
            return _namespace switch
            {
                null => "*",
                _ => _namespace
            };
        }

        public override int GetHashCode()
        {
            return _namespace switch
            {
                null => 0,
                _ => _namespace.GetHashCode()
            };
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is OperationNamespace other && Equals(other);
        }

        public bool Equals(OperationNamespace other)
        {
            return EqualityComparer<string>.Default.Equals(this._namespace, other._namespace);
        }
        #endregion

        #region Parse
        public static bool TryParse(string text, out Result<OperationNamespace> result)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                result = default;
                return false;
            }
            else if (Pattern.IsMatch(text))
            {
                result = new(text);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Result<OperationNamespace> ParseResult(string text)
        {
            _ = TryParse(text, out var result);
            return result;
        }

        public static OperationNamespace Parse(string text)
        {
            if (TryParse(text, out var result))
                return result.Resolve();

            else throw new FormatException($"Invalid namespace format: {text}");
        }
        #endregion

        public static implicit operator OperationNamespace(string @namespace) => new(@namespace);

        public static implicit operator string(OperationNamespace @namespace) => @namespace.ToString()!;

        public static bool operator ==(OperationNamespace left, OperationNamespace right) => left.Equals(right);

        public static bool operator !=(OperationNamespace left, OperationNamespace right) => !left.Equals(right);
    }

}
