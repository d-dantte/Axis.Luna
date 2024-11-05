using System;

namespace Axis.Luna.Common
{
    public class CharSequenceReader
    {
        private int index = 0;
        private readonly CharSequence text;

        public int CurrentIndex => index;

        public bool CanRead => index < text.Length;

        public CharSequenceReader(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            this.text = text;
        }

        public CharSequenceReader Reset(int index = 0)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0);

            this.index = index;
            return this;
        }

        public CharSequenceReader Back(int steps = 1)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(steps, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(steps, index);

            index -= steps;
            return this;
        }

        public CharSequenceReader Advance(int steps = 1)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(steps, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(steps, text.Length - index);

            index += steps;
            return this;
        }

        #region Read
        /// <summary>
        /// Reads exactly <paramref name="count"/> characters from the unerlying string, at the current index.
        /// If <paramref name="count"/> characters cannot be read, the Buffer's index is not advanced.
        /// </summary>
        /// <param name="count">Number of characters to be read</param>
        /// <param name="chars">Characters that were read</param>
        /// <returns>True if exactly <paramref name="count"/> characters were read, false otherwise.</returns>
        public bool TryReadExactly(int count, out CharSequence chars)
        {
            if (TryPeekExactly(count, out chars))
            {
                index += chars.Length;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads at most <paramref name="maxReadCount"/> number of characters from the underlying string, at the current index.
        /// If read was successful, advance the buffer's index by however many characters were read.
        /// </summary>
        /// <param name="maxReadCount">Maximum number of characters to be read</param>
        /// <param name="chars">The characters that were read</param>
        /// <returns>True if at least 1 character (up to <paramref name="maxReadCount"/>) was read, false otherwise.</returns>
        public bool TryRead(int maxReadCount, out CharSequence chars)
        {
            if (TryPeek(maxReadCount, out chars))
            {
                index += chars.Length;
                return true;
            }

            return false;
        }

        /// <summary>
        /// In addition to reading the characters, invoke the predicate.
        /// </summary>
        /// <param name="count">Exact number of characters to be read</param>
        /// <param name="predicate">The predicate to invoke on successful read</param>
        /// <param name="chars">The characters that were read</param>
        /// <returns>True if both read and predicate succeeded, False otherwise</returns>
        public bool TryReadExactly(
            int count,
            Func<CharSequence, bool> predicate,
            out CharSequence chars)
        {
            if (TryPeekExactly(count, predicate, out chars))
            {
                index += chars.Length;
                return true;
            }

            return false;
        }

        /// <summary>
        /// In addition to reading the characters, invoke the predicate.
        /// </summary>
        /// <param name="maxReadCount">Max number of characters to be read</param>
        /// <param name="predicate">The predicate to invoke on successful read</param>
        /// <param name="chars">The characters that were read</param>
        /// <returns>True if both read and predicate succeeded, False otherwise</returns>
        public bool TryRead(
            int maxReadCount,
            Func<CharSequence, bool> predicate,
            out CharSequence chars)
        {
            if (TryPeek(maxReadCount, predicate, out chars))
            {
                index += chars.Length;
                return true;
            }

            return false;
        }
        #endregion

        #region Peek
        /// <summary>
        /// Peeks exactly <paramref name="count"/> characters from the unerlying string, at the current index.
        /// </summary>
        /// <param name="count">Number of characters to be peeked</param>
        /// <param name="chars">Characters that were peeked</param>
        /// <returns>True if exactly <paramref name="count"/> characters were peeked, false otherwise.</returns>
        public bool TryPeekExactly(int count, out CharSequence chars)
        {
            chars = default;

            if (count > (text.Length - index))
                return false;

            chars = text[index..(index + count)];
            return true;
        }

        /// <summary>
        /// Peeks at most <paramref name="maxPeekCount"/> number of characters from the underlying string, at the current index.
        /// </summary>
        /// <param name="maxPeekCount">Maximum number of characters to be peeked</param>
        /// <param name="chars">The characters that were peeked</param>
        /// <returns>True if at least 1 character (up to <paramref name="maxPeekCount"/>) was peeked, false otherwise.</returns>
        public bool TryPeek(int maxPeekCount, out CharSequence chars)
        {
            chars = default;
            var maxCount = text.Length - index;

            if (maxCount == 0)
                return false;

            if (maxPeekCount > maxCount)
                maxPeekCount = maxCount;

            chars = text[index..(index + maxPeekCount)];
            return true;
        }

        /// <summary>
        /// In addition to successfully peeking <paramref name="count"/> characters, invoke the given predicate.
        /// </summary>
        /// <param name="count">The number of characters to be peeked</param>
        /// <param name="predicate">The predicate to invoke if the peek operation was successful</param>
        /// <param name="chars">The characters that were peeked. Only ever <c>default</c> if the peek operation failed</param>
        /// <returns>True if peek and the predicate were both successful, False otherwise</returns>
        public bool TryPeekExactly(
            int count,
            Func<CharSequence, bool> predicate,
            out CharSequence chars)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return TryPeekExactly(count, out chars) && predicate.Invoke(chars);
        }

        /// <summary>
        /// In addition to successfully peeking <paramref name="count"/> characters, invoke the given predicate.
        /// </summary>
        /// <param name="count">The max number of characters to be peeked</param>
        /// <param name="predicate">The predicate to invoke if the peek operation was successful</param>
        /// <param name="chars">The characters that were peeked. Only ever <c>default</c> if the peek operation failed</param>
        /// <returns>True if peek and the predicate were both successful, False otherwise</returns>
        public bool TryPeek(
            int maxReadCount,
            Func<CharSequence, bool> predicate,
            out CharSequence chars)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return TryPeek(maxReadCount, out chars) && predicate.Invoke(chars);
        }
        #endregion
    }
}
