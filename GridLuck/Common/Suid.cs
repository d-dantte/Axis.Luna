using GridLuck.Common.Extensions;
using GridLuck.Common.Results;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace GridLuck.Common
{
    /// <summary>
    /// 176 bit - (22 bytes) Serial Unique ID
    /// </summary> 
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Suid :
        IEquatable<Suid>,
        IComparable<Suid>
    {
        internal static readonly Regex Pattern = new(
            @"^[a-fA-F0-9]{8}-[a-fA-F0-9]{8}-[a-fA-F0-9]{2}-[a-fA-F0-9]{2}(-[a-fA-F0-9]{4}){4}-[a-fA-F0-9]{8}$",
            RegexOptions.Compiled);

        private readonly ulong _timestamp;
        private readonly ushort _prefix;
        private readonly ulong _urandom;
        private readonly uint _irandom;

        #region Default
        public bool IsDefault => _timestamp == 0 && _urandom == 0 && _irandom == 0;

        public static Suid Default => default;
        #endregion

        #region Construction
        public Suid() : this((ulong)DateTimeOffset.Now.Ticks, 0, RandomULong(), RandomUInt())
        {
        }

        public Suid(ulong timestamp) : this(timestamp, 0, RandomULong(), RandomUInt())
        {
        }

        public Suid(ulong timestamp, ushort prefix) : this(timestamp, prefix, RandomULong(), RandomUInt())
        {
        }

        public Suid(ulong timestamp, ushort prefix, ulong urandom, uint irandom)
        {
            _timestamp = timestamp;
            _prefix = prefix;
            _urandom = urandom;
            _irandom = irandom;
        }

        public static Suid Create() => new();

        /// <summary>
        /// Creates a new Suid from an array of bytes arranged sequentially: timestamp(8) > prefix(2) > random(8)
        /// </summary>
        /// <param name="bytes">The bytes</param>
        /// <returns>The new Suid</returns>
        public static Suid Create(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return Create(new ReadOnlySpan<byte>(bytes));
        }

        /// <summary>
        /// Creates a new Suid from an array of bytes arranged sequentially: timestamp(8) > prefix(2) > random(8)
        /// </summary>
        /// <param name="bytes">The bytes</param>
        /// <returns>The new Suid</returns>
        public static Suid Create(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 22)
                throw new ArgumentOutOfRangeException(
                    nameof(bytes),
                    $"Invalid byte sequence length [expeted: 18, found: {bytes.Length}]");

            return new(
                prefix: BitConverter.ToUInt16(bytes[8..10]),
                timestamp: BitConverter.ToUInt64(bytes[..8]),
                urandom: BitConverter.ToUInt64(bytes[10..18]),
                irandom: BitConverter.ToUInt32(bytes[18..]));
        }

        /// <summary>
        /// Creates a new <see cref="Suid"/> from an array of bytes arranged sequentially: 8 (timestamp), 8 (random), 4 (random)
        /// </summary>
        /// <param name="prefix">The prefix value</param>
        /// <param name="bytes">The bytes</param>
        /// <returns>The new Suid</returns>
        public static Suid Create(ushort prefix, byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            return Create(prefix, new ReadOnlySpan<byte>(bytes));
        }

        /// <summary>
        /// Creates a new <see cref="Suid"/> from an array of bytes arranged sequentially: 8 (timestamp), 8 (random), 4 (random)
        /// </summary>
        /// <param name="prefix">The prefix value</param>
        /// <param name="bytes">The bytes</param>
        /// <returns>The new Suid</returns>
        public static Suid Create(ushort prefix, ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 20)
                throw new ArgumentOutOfRangeException(
                    nameof(bytes),
                    $"Invalid byte sequence length [expeted: 16, found: {bytes.Length}]");

            return new(
                prefix: prefix,
                timestamp: BitConverter.ToUInt64(bytes[..8]),
                urandom: BitConverter.ToUInt64(bytes[8..16]),
                irandom: BitConverter.ToUInt32(bytes[16..]));
        }

        public static implicit operator Suid(string text) => Parse(text);

        public static implicit operator string(Suid id) => id.ToString();

        public byte[] ToBytes() => [
            ..BitConverter.GetBytes(_timestamp),
            ..BitConverter.GetBytes(_prefix),
            ..BitConverter.GetBytes(_urandom),
            ..BitConverter.GetBytes(_irandom),
        ];
        #endregion

        #region Comparable
        public int CompareTo(Suid other)
        {
            return _timestamp.CompareTo(other._timestamp) switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => _prefix.CompareTo(other._prefix) switch
                {
                    < 0 => -1,
                    > 0 => 1,
                    _ => _urandom.CompareTo(other._urandom) switch
                    {
                        < 0 => -1,
                        > 0 => 1,
                        _ => _irandom.CompareTo(other._irandom)
                    }
                }
            };
        }
        #endregion

        #region Overrides
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Suid other)
        {
            return _timestamp == other._timestamp
                && _prefix == other._prefix
                && _urandom == other._urandom
                && _irandom == other._irandom;
        }

        public override int GetHashCode() => HashCode.Combine(_timestamp, _prefix, _urandom, _irandom);

        public override string ToString()
        {
            var tsText = _timestamp.ToString("X16");
            var prefixText = _prefix.ToString("X4");
            var urandomText = _urandom.ToString("X16");
            var irandomText = _irandom.ToString("X8");

            return
                $"{tsText[..8]}-{tsText[8..]}"
                + $"-{prefixText[..2]}-{prefixText[2..]}"
                + $"-{urandomText[..4]}-{urandomText[4..8]}"
                + $"-{urandomText[8..12]}-{urandomText[12..]}"
                + $"-{irandomText}";
        }
        #endregion

        #region Parse
        public static Suid Parse(string text)
        {
            _ = TryParse(text, out var result);
            return result.Resolve();
        }

        public static bool TryParse(string text, out Result<Suid> result)
        {
            if (text.Length == 52 && Pattern.IsMatch(text))
            {
                try
                {
                    var undelimited = text.Replace("-", "");
                    result = new Suid(
                        ulong.Parse(undelimited[..16], NumberStyles.HexNumber),
                        ushort.Parse(undelimited[16..20], NumberStyles.HexNumber),
                        ulong.Parse(undelimited[20..36], NumberStyles.HexNumber),
                        uint.Parse(undelimited[36..], NumberStyles.HexNumber));
                    return true;
                }
                catch (Exception error)
                {
                    result = error;
                    return false;
                }
            }
            else
            {
                result = new FormatException($"Invalid {typeof(Suid)} format: {text}");
                return false;
            }
        }
        #endregion

        #region Helpers
        private static ulong RandomULong()
        {
            var bytes = new byte[8];
            RandomNumberGenerator.Fill(bytes);
            return BitConverter.ToUInt64(bytes.AsReadOnlySpan());
        }

        private static uint RandomUInt()
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            return BitConverter.ToUInt32(bytes.AsReadOnlySpan());
        }
        #endregion

        #region Operators
        public static bool operator ==(Suid left, Suid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Suid left, Suid right)
        {
            return !(left == right);
        }

        public static bool operator <(Suid left, Suid right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Suid left, Suid right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Suid left, Suid right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Suid left, Suid right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion
    }
}