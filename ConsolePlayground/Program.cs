using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;
using System.Security.Cryptography;

namespace ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                for (int cnt = 0; cnt < 50; cnt++)
                {
                    Console.WriteLine(rng.RandomSignedInt());
                }

                Console.ReadKey();
                for (int cnt = 0; cnt < 50; cnt++)
                {
                    Console.WriteLine(rng.RandomInt(10, 20));
                }
            }

            Console.ReadKey();
        }
    }
}
