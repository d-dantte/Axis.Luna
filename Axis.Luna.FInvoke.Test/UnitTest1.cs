using System;
using Xunit;


namespace Axis.Luna.FInvoke.Test
{
    public class UnitTest1
    {
        private readonly Xunit.Abstractions.ITestOutputHelper Console;

        public UnitTest1(Xunit.Abstractions.ITestOutputHelper helper)
        {
            Console = helper;
        }

        [Fact]
        public void PerformanceTest()
        {
            var type = typeof(SampleClass);
            var instance = new SampleClass();
            
            const int callCount = 1000000;

            //Action1
            var method = type.GetMethod("Action1");
            instance.Invoke(method); instance.Invoke(method); //warmup
            var start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method);
            var invokerTime = DateTime.Now - start;

            var dinvoker = InstanceInvoker.InvokerFor(method);
            dinvoker.Func(instance, null); //warmup
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) dinvoker.Func(instance, null);
            var dinvokerTime = DateTime.Now - start;

            var del = method.CreateDelegate(typeof(Action), instance);
            var ddel = (Action)del;
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) ddel.Invoke();
            var directDelegateTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) del.DynamicInvoke();
            var dynamicDelegateTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action1();
            var directTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) method.Invoke(instance, new object[0]);
            var reflectionTime = DateTime.Now - start;

            dynamic dinstance = instance;
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) dinstance.Action1();
            var dynamicTime = DateTime.Now - start;

            var output = "Action1 stats:\n";
            output += string.Join(
                "\n",
                new[]
                {
                    $"reflection-time: {reflectionTime}",
                    $"invoker-time: {invokerTime}",
                    $"dynamic-invoker-time: {dinvokerTime}",
                    $"direct-delegate-time: {directDelegateTime}",
                    $"dynamic-delegate-time: {dynamicDelegateTime}",
                    $"dynamic-time: {dynamicTime}",
                    $"direct-time: {directTime}",
                })
                + $"], average-invoker-time: {new TimeSpan(dinvokerTime.Ticks / callCount)}";

            Console.WriteLine(output+"\n\n");

            //Action2
            method = type.GetMethod("Action2");
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action2(654);
            directTime = DateTime.Now - start;

            dinstance = instance;
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) dinstance.Action2(654);
            dynamicTime = DateTime.Now - start;

            Console.WriteLine($"Action2 stats [call-count: {callCount},  invoker-time: {invokerTime}, dynamic-time: {dynamicTime} direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Action3
            object i = 654, l = 654L, s = "me";
            method = type.GetMethod("Action3");
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654, 654L, "me");
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action3((int)i, (long)l, (string)s);
            directTime = DateTime.Now - start;

            dinstance = instance;
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) dinstance.Action3((int)i, (long)l, (string)s);
            dynamicTime = DateTime.Now - start;

            Console.WriteLine($"Action3 stats [call-count: {callCount},  invoker-time: {invokerTime}, dynamic-time: {dynamicTime} direct-time: {directTime}, average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}]");

            //Action4
            method = type.GetMethod("Action4").MakeGenericMethod(typeof(string));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action4<string>(654);
            directTime = DateTime.Now - start;

            dinstance = instance;
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) dinstance.Action4<string>(654);
            dynamicTime = DateTime.Now - start;

            Console.WriteLine($"Action4 stats [call-count: {callCount},  invoker-time: {invokerTime}, dynamc-time: {dynamicTime}  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");


            //Func1
            method = type.GetMethod("Func1");
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func1();
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func1 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func2
            method = type.GetMethod("Func2");
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func2(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func2 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func3
            method = type.GetMethod("Func3");
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654, 654l, "me");
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func3(654, 654L, "me");
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func3 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func4
            method = type.GetMethod("Func4").MakeGenericMethod(typeof(string));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Invoke(method, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func4<string>(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func4 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            Console.WriteLine("\n\n Total Call Count:" + instance.g);

        }
    }

    public class SampleClass
    {
        public int g = 0;
        #region instance
        public void Action1()
        {
            g++;
        }

        public void Action2(int x)
        {
            g++;
        }

        public void Action3(int x, long y, string z)
        {
            g++;
        }

        public void Action4<X>(int b)
        {
            g++;
        }


        public string Func1()
        {
            g++;
            return nameof(Func1);
        }


        public int Func2(int x)
        {
            g++;
            return nameof(Func2).GetHashCode();
        }

        public object Func3(int x, long y, string z)
        {
            g++;
            return nameof(Func3);
        }

        public string Func4<X>(int b)
        {
            g++;
            return nameof(Func4);
        }
        #endregion


        #region Statics
        public static void StaticAction1()
        {
        }

        public static void StaticAction2(int x)
        {
        }

        public static void StaticAction3(int x, long y, string z)
        {
        }

        public static void StaticAction4<X>(int b)
        {
        }


        public static string StaticFunc1() => nameof(StaticFunc1);

        public static string StaticFunc2(int x) => nameof(StaticFunc2);

        public static string StaticFunc3(int x, long y, string z) => nameof(StaticFunc3);

        public static string StaticFunc4<X>(int b) => nameof(StaticFunc4);
        #endregion
    }
}
