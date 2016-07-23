﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Axis.Luna.Extensions;
using Expressions;
using static Axis.Luna.Void;

namespace Axis.Luna.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            MetaTypes.@void t = @void;
            var x = @void;
            var r = new R { a = 6, b = 45 };
            var bexp = new DynamicExpression("Abs(a) * Sqrt(b)", ExpressionLanguage.Csharp).Bind(r);
            Console.WriteLine(bexp.Invoke(r));
            
        }

        [TestMethod]
        public void TestMethod2()
        {
            var r = new Random();
            Enumerable.Range(0, 1000)
                      .Select(rr => Math.Round(r.NextGaussian(300, 10), 1))
                      .OrderBy(rr => rr)
                      .ToList()
                      .ForAll((cnt, x) => Console.WriteLine($"{cnt}.\t{x}"));
        }

        [TestMethod]
        public void TestMethod3()
        {
            var json = JsonConvert.SerializeObject(new X { XStuff = "sldkfjle" }, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            var dis = JsonConvert.DeserializeObject<dynamic>(json);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var att = typeof(Thing).GetMethod("GetSomething").GetCustomAttributes();
            Console.WriteLine(att == null ? "null" : att.ToString());
        }

        [TestMethod]
        public void TestMethod5()
        {
            var iar = new int[] { 2,1,4,4,5,9};
            var sar = new string[] { "dlak", "dflkea", "pot4ie", "thv" };

            iar.PairWith(sar, true).ForAll((cnt, x) => Console.WriteLine($"[{x.Key}: {x.Value}]"));
        }

        public static void Method(long x)
        {

        }
        public static void Method(short x)
        {

        }
        public static void Method<Stuff>(short x)
        {
            var g = Call(() => Call_());
        }

        public static Operation<Operation<int>> Call_() { throw new Exception(); }

        public static Operation<T> Call<T>(Func<T> abcd)
        {
            return Operation.Run(() => abcd());
        }
        public static Operation<T> Call<T>(Func<Operation<T>> abcd)
        {
            try
            {
                return abcd();
            }
            catch(Exception e)
            {
                return Operation.Fail<T>(e);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class MyAtt: Attribute
    {
    }

    public interface IThing
    {
        [MyAtt]
        int GetSomething();
    }
    public class Thing : IThing
    {
        public int GetSomething() => 0;
    }

    public static class rrr
    {
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }
    }

    public class R: ExpressionContext
    {
        public R()
        {
            Imports.Add(new Import(typeof(Math)));

            Owner = this;
        }

        public int a { get; set; }
        public double b { get; set; }
    }

    public class X: IDisposable
    {
        public string XStuff { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class Y: IDisposable
    {
        public string YStuff { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }


    public class SomeEnum: StructuredEnum
    {
        public static readonly SomeEnum
        First,
        Second,
        Third;

        #region Properties
        public string Name { get; private set; }
        #endregion

        #region Init
        private SomeEnum() { }
        static SomeEnum()
        {
            SynthesizeValues(n => new SomeEnum { Name = n });
        }
        #endregion

        public override string ToString() => Name;
    }

    public class SomeClass<V>
    {
        public V Stuff { get; set; }
        public int OtherStuff { get; set; }
    }
}
