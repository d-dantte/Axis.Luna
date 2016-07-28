using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Axis.Luna.Extensions;
using Expressions;
using static Axis.Luna.Void;
using System.ComponentModel;
using System.Threading;
using System.Linq.Expressions;

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
            //var bexp = new DynamicExpression("Abs(a) * Sqrt(b)", ExpressionLanguage.Csharp).Bind(r);
            //Console.WriteLine(bexp.Invoke(r));
            
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

        [TestMethod]
        public void TestMethod6()
        {
            var tt = new Notifiable();
            var method = typeof(Notifiable).GetMethod("abcd");

            PropertyChangedEventHandler @delegate = tt.abcd;
            var param = @delegate.Method.GetParameters().Last();

            // (t, s, a) => ((Notifiable)t).SomeMethod(s, a); or Something(s, a);
            var t = Expression.Parameter(typeof(object), "target");
            var s = Expression.Parameter(typeof(object), "source");
            var a = Expression.Parameter(typeof(object), "args");

            var callExp = Expression.Call(Expression.Convert(t, @delegate.Target.GetType()), method, s, Expression.Convert(a, param.ParameterType));

            var lambda = //handler.Method.IsStatic ? Expression.Lambda(callExp, s, a) :
                         Expression.Lambda(callExp, t, s, a);

            var x = (Action<object, object, object>)lambda.Compile();

            var st = DateTime.Now;
            x.Invoke(tt, null, null);
            Console.WriteLine($"Called in : {DateTime.Now - st}");
        }

        [TestMethod]
        public void TestMethod7()
        {
            var n1 = new Notifiable();
            var n2 = new Notifiable();

            Console.WriteLine(n1 == n2);

            PropertyChangedEventHandler pc = null;

            pc += n1.abcd;
            Console.WriteLine(pc.GetInvocationList().Length);

            pc -= n2.abcd;
            Console.WriteLine(pc.GetInvocationList().Length);
        }

        public static Del Remove<Del>(Delegate source, PropertyChangedEventHandler target)
        {
            var t = source.GetInvocationList().FirstOrDefault(d => d.Target == target.Target && d.Method == target.Method);
            return Delegate.Remove(source, t).As<Del>();
        }


        public void OnPropertyChange(object sender, PropertyChangedEventArgs arg)
        {
        }
        public void OnPropertyChange2(object sender, PropertyChangedEventArgs arg)
        {
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

    public class Notifiable: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void abcd(object x, PropertyChangedEventArgs e)
        {
            Console.WriteLine("called");
        }

        public static void efgh(object x, PropertyChangedEventArgs e)
        {
            Console.WriteLine("called static");
        }

        public override bool Equals(object obj)
        {
            if (obj is Notifiable) return true;
            else return false;
        }
        public override int GetHashCode() => 1;

        public static bool operator ==(Notifiable a, Notifiable b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;

            else return a?.Equals(b) ?? false;
        }

        public static bool operator !=(Notifiable a, Notifiable b) => !(a == b);

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
