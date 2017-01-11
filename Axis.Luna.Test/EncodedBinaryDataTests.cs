using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Axis.Luna.Extensions;
using System.Text;

namespace Axis.Luna.Test
{
    [TestClass]
    public class EncodedBinaryDataTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new EncodedBinaryData(Enumerable.Range(0, 2000).Select(_r => (byte)random.Next(128)).ToArray(), "something.txt");

            Console.WriteLine(data.Data);
            Console.WriteLine(data.Name);
            Console.WriteLine(data.Mime);
        }

        [TestMethod]
        public void MapShuffler()
        {
            var alpha = Enumerable.Range(0, 52).Select(x => x < 26? 'A'+x: 'a'+(x-26)).Select(_x => (char)_x).ToArray();
            var num = Enumerable.Range(0, 10).ToArray();

            var shuffledAlpha = alpha.Shuffle()
                .Aggregate(new StringBuilder(), (x, y) => x.Append("'").Append(y).Append("',"));
            Console.WriteLine(shuffledAlpha.ToString().TrimEnd(","));

            var shuffledNum = num.Shuffle()
                .Aggregate(new StringBuilder(), (x, y) => x.Append("'").Append(y).Append("',"));
            Console.WriteLine(shuffledNum.ToString().TrimEnd(","));
        }
    }
}
