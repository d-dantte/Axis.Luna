using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Axis.Luna.Extensions.Test
{
    [TestClass]
    public class DynamicMethodInvokerTests
    {
        [TestMethod]
        public void ParameterTypeTest()
        {
            var obj = "value";
            var method = typeof(DynamicMethodInvokerTests).GetMethod(nameof(SampleStringMethod));
            var gmethod = method.MakeGenericMethod(typeof(string));
            this.CallAction(gmethod, obj);
        }

        [TestMethod]
        public void CSharpImplicitOperatorReflectiveCallTest()
        {
            //use fast call to call a sample method that takes Digit, but pass a byte in as the parameter.
            byte @byte = 9;
            var digit = new Digit(1);
            //var xdigit = (Digit) (object) @byte; <-- wont work!!

            var converter = ConvertTo<Digit>(typeof(byte));
            digit = converter.Invoke(@byte);

            SampleDigitMethod(digit);
            SampleDigitMethod((Digit)@byte);

            var sampleDigitMethod = GetType().GetMethod(nameof(SampleDigitMethod));

            this.CallAction(sampleDigitMethod, digit);
            this.CallAction(sampleDigitMethod, @byte);


            var sampleLongMethod = GetType().GetMethod(nameof(SampleLongMethod));
            var sampleIntMethod = GetType().GetMethod(nameof(SampleIntMethod));


            this.CallAction(sampleLongMethod, 8);
            this.CallAction(sampleIntMethod, 8L);
        }

        public void SampleStringMethod<T>(T param)
        {
            Console.WriteLine(param);
        }

        public void SampleDigitMethod(Digit d)
        {
            Console.WriteLine(d);
        }

        public void SampleIntMethod(int i)
        {
            Console.WriteLine(i);
        }

        public void SampleLongMethod(long l)
        {
            Console.WriteLine(l);
        }

        public Func<object, To> ConvertTo<To>(Type fromType)
        {
            var argParam = Expression.Parameter(typeof(object), "arg");
            var unboxed = Expression.Unbox(argParam, fromType);
            var converted = Expression.Convert(unboxed, typeof(To));
            return (Func<object, To>)Expression
                .Lambda(typeof(Func<object, To>), converted, argParam)
                .Compile();
        }
    }

    public struct Digit
    {
        private byte value;

        public Digit(byte value)  //constructor
        {
            if (value > 9)
            {
                throw new System.ArgumentException();
            }
            this.value = value;
        }

        public static implicit operator byte(Digit d)  // implicit digit to byte conversion operator
        {
            return d.value;  // implicit conversion
        }

        public static implicit operator Digit(byte b)  // implicit byte to Digit conversion operator
        {
            return new Digit(b);  // implicit conversion
        }

        //public static explicit operator Digit(byte b)  // implicit byte to Digit conversion operator
        //{
        //    return new Digit(b);  // implicit conversion
        //}

        public override string ToString() => value.ToString();
    }
}
