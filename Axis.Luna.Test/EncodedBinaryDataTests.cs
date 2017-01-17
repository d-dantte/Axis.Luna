using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Axis.Luna.Extensions;
using System.Text;
using Newtonsoft.Json;

using static Axis.Luna.Extensions.EnumerableExtensions;

namespace Axis.Luna.Test
{
    [TestClass]
    public class EncodedBinaryDataTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new EncodedBinaryData(Enumerable.Range(0, 2000).Select(_r => (byte)random.Next(128)).ToArray(), "application/text");

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

        [TestMethod]
        public void Serializing()
        {
            var settings = new JsonSerializerSettings
            {
                //Converters = Enumerate<JsonConverter>()
                //.Append(new Axis.Apollo.Json.TimeSpanConverter())
                //.Append(new Axis.Apollo.Json.DateTimeConverter())
                //.ToList(),
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                StringEscapeHandling = StringEscapeHandling.Default
            };

            var random = new Random(Guid.NewGuid().GetHashCode());
            var data = new EncodedBinaryData(Enumerable.Range(0, 2000).Select(_r => (byte)random.Next(128)).ToArray(), "application/text");

            var json = JsonConvert.SerializeObject(data, settings);

            Console.WriteLine(json);

            var xdata = JsonConvert.DeserializeObject<EncodedBinaryData>(json, settings);
        }
    }
}
