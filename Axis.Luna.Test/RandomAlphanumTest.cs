using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using Axis.Luna.Extensions;

namespace Axis.Luna.Test
{
    [TestClass]
    public class RandomAlphanumTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            var randomString = RandomAlphaNumericGenerator.RandomAlpha(4, r);
            Console.WriteLine("Random string: " + randomString);

            var collisions = 0;
            var iterations = 8000000;
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var sample = RandomAlphaNumericGenerator.RandomAlpha(4, r);
                if (sample.Any(_char => char.IsDigit(_char)))
                {
                    Console.WriteLine($"bad random-alpha detected: {sample}");
                    return;
                }
                if (sample == randomString) collisions++;
            }

            Console.WriteLine($"in {iterations} iterations, {collisions} collissions were found.");


        }
        [TestMethod]
        public void TestMethod3()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            
            var iterations = 8000000;
            var buffer = new Dictionary<string,int>();
            for (int cnt = 0; cnt < iterations; cnt++)
            {
                var value = RandomAlphaNumericGenerator.RandomAlpha(50, r);

                var occurence = buffer.GetOrAdd(value, _v => 0);
                buffer[value] = occurence + 1;
            }


            var x = buffer.Where(_v => _v.Value > 1);
            Console.WriteLine("total repetitions: " + x.Count());
            //x.ForAll((cnt, _kvp) => Console.WriteLine($"{_kvp.Key} : {_kvp.Value}"));
        }

        public class Occurences
        {
            public string String { get; set; }
            public int Occurence { get; set; }

            public Occurences(string value)
            {
                this.String = value;
                this.Occurence = 1;
            }
        }



        [TestMethod]
        public void TestMethod2()
        {
            for (int cnt = 0; cnt < 10000; cnt++)
                Console.WriteLine(RandomAlphaNumericGenerator.RandomAlpha(3));
        }
    }
}
