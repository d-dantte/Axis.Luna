using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputArray = Enumerable
                .Range(1, 1000)
                .Select(_v => RandomStep())
                .ToArray();

            var payload = Step(inputArray, 0, new Payload());

            Console.WriteLine($"Valley count: {payload.ValleyCount}");
            Console.WriteLine($"Hill count: {payload.HillCount}");
            Console.WriteLine($"Final Altitude: {payload.Altitude}");

            Console.ReadKey();
        }

        //Functional method
        public static Payload Step(char[] steps, int stepCount, Payload payload)
        {
            if (stepCount == steps.Length)
                return payload;
            else
            {
                var step = steps[stepCount];
                var altitude = step == 'U' ? payload.Altitude + 1 :
                               step == 'D' ? payload.Altitude - 1 :
                               throw new Exception("invalid step");
                return Step(steps, stepCount + 1, new Payload
                {
                    Altitude = altitude,
                    HillCount = payload.HillCount + (payload.Altitude == 0 && altitude > 0 ? 1 : 0),
                    ValleyCount = payload.ValleyCount + (payload.Altitude == 0 && altitude < 0 ? 1 : 0)
                });
            }
        }

        public static char RandomStep()
        {
            var r = new Random(Guid.NewGuid().GetHashCode());
            return r.Next() % 2 == 0 ? 'U' : 'D';
        }
    }

    /// <summary>
    /// this, you could use a tuple to implement
    /// </summary>
    public struct Payload
    {
        public long Altitude { get; set; }
        public long HillCount { get; set; }
        public long ValleyCount { get; set; }
    }
}
