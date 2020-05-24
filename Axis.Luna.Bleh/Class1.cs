using System;

namespace Axis.Luna.Bleh
{
    public class Class1
    {
        public static int StaticFunc() => 2;

        public static void StaticAction(int x, string y) { }



        public int InstanceFunc(int x, string y) => 1;

        public void InstanceAction() 
        {
            var x = 5 * 6;
            var y = x++;
            Console.WriteLine(y);
            x++;
            return;
        }

        public string InstanceFunc(int x, DateTime y, string z, params bool[] args) => "65554";


        public void __InstanceAction(object instance, object[] args)
        {
            (instance as Class1).InstanceAction();
        }

        public void __StaticAction(Type type, object[] args)
        {
            Class1.StaticAction(
                (int)args[0],
                args[1] as string);
        }

        public object __InstanceFunc(object instance, object[] args)
        {
            return (instance as Class1).InstanceFunc(
                (int)args[0],
                args[1] as string);
        }

        public object __InstanceFunc2(object instance, object[] args)
        {
            return (instance as Class1).InstanceFunc(
                (int)args[0],
                (DateTime)args[1],
                args[2] as string,
                args[3] as bool[]);
        }

        public object __InstanceFunc3(object instance, object[] args)
        {
            return (instance as Class1).InstanceFunc(
                (int)args[0],
                (DateTime)args[1],
                args[2] as string,
                true, false, true, false);
        }

        public object __StaticFunc(Type type, object[] args)
        {
            return Class1.StaticFunc();
        }
    }
}
