using System.IO;
using System.Text;

namespace Axis.Luna.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// https://stackoverflow.com/a/19283954
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream PeekTextEncoding(
            this Stream stream,
            out Encoding encoding,
            Encoding defaultEncoding)
        {
            var bufferedStream = new BufferedStream(stream);
            var bom = new byte[4];
            var length = bufferedStream.Read(bom, 0, bom.Length);

            // UTF-32 LE
            if (length == 4 && bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0)
                encoding = Encoding.UTF32;

            // UTF-32 BE
            else if (length == 4 && bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                encoding = new UTF32Encoding(true, true);

            // UTF-8
            else if (length >= 3 && bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                encoding = Encoding.UTF8;

            // UTF-16 LE
            else if (length >= 2 && bom[0] == 0xff && bom[1] == 0xfe)
                encoding = Encoding.Unicode;

            // UTF-16 BE
            else if (length >= 2 && bom[0] == 0xfe && bom[1] == 0xff)
                encoding = Encoding.BigEndianUnicode;

            else encoding = defaultEncoding ?? Encoding.ASCII;

            bufferedStream.Position = 0;
            return bufferedStream;
        }

        /// <summary>
        /// https://stackoverflow.com/a/19283954
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Stream PeekTextEncoding(
            this Stream stream,
            out Encoding encoding)
            => PeekTextEncoding(stream, out encoding, null);
    }
}
