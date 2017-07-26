using Axis.Luna.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Axis.Luna.Test.Utils
{
    [TestClass]
    public class DynamicMethodInvokerTest
    {
        [TestMethod]
        public void PerformanceTest()
        {
            var type = typeof(SampleClass);
            var instance = new SampleClass();

            const int callCount = 10000;

            //Action1
            var invoker = new DynamicMethodInvoker(type.GetMethod("Action1"));
            var start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeAction(instance);
            var invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action1();
            var directTime = DateTime.Now - start;

            Console.WriteLine($"Action1 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Action2
            invoker = new DynamicMethodInvoker(type.GetMethod("Action2"));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeAction(instance, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action2(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Action2 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Action3
            invoker = new DynamicMethodInvoker(type.GetMethod("Action3"));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeAction(instance, 654, 654l, "me");
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action3(654, 654L, "me");
            directTime = DateTime.Now - start;

            Console.WriteLine($"Action3 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Action4
            invoker = new DynamicMethodInvoker(type.GetMethod("Action4").MakeGenericMethod(typeof(string)));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeAction(instance, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Action4<string>(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Action4 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");


            //Func1
            invoker = new DynamicMethodInvoker(type.GetMethod("Func1"));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeFunc(instance);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func1();
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func1 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func2
            invoker = new DynamicMethodInvoker(type.GetMethod("Func2"));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeFunc(instance, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func2(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func2 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func3
            invoker = new DynamicMethodInvoker(type.GetMethod("Func3"));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeFunc(instance, 654, 654l, "me");
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func3(654, 654L, "me");
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func3 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks / callCount)}");

            //Func4
            invoker = new DynamicMethodInvoker(type.GetMethod("Func4").MakeGenericMethod(typeof(string)));
            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) invoker.InvokeFunc(instance, 654);
            invokerTime = DateTime.Now - start;

            start = DateTime.Now;
            for (int cnt = 0; cnt < callCount; cnt++) instance.Func4<string>(654);
            directTime = DateTime.Now - start;

            Console.WriteLine($"Func4 stats [call-count: {callCount},  invoker-time: {invokerTime},  direct-time: {directTime}], average-invoker-time: {new TimeSpan(invokerTime.Ticks/callCount)}");

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


        public string Func2(int x)
        {
            g++;
            return nameof(Func2);
        }

        public string Func3(int x, long y, string z)
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
