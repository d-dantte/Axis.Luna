using Axis.Luna.Extensions.Benchmark.Types;
using BenchmarkDotNet.Attributes;
using System.Reflection;

namespace Axis.Luna.Extensions.Benchmark
{
    [Config(typeof(AntiVirusFriendlyConfig))]
    [MemoryDiagnoser]
    public class TypeExtension
    {
        internal static FieldAccessorTestClass fatc = new()
        {
        };
        internal static FieldInfo PublicClassField;
        internal static FieldInfo PrivateClassField;
        internal static FieldInfo InternalClassField;
        internal static FieldInfo ProtectedClassField;



        internal static FieldAccessorTestStruct fats = new()
        {
        };
        internal static object fatsRef = fats;
        internal static FakeLargeStruct fls = new();
        internal static FieldInfo PublicStructField;
        internal static FieldInfo PrivateStructField;
        internal static FieldInfo InternalStructField;
        internal static FieldInfo FakeLargeStructField;

        static TypeExtension()
        {
            // class
            var fields = typeof(FieldAccessorTestClass).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PublicClassField = fields.First(f => f.Name.Equals("PublicField"));
            PrivateClassField = fields.First(f => f.Name.Equals("PrivateField"));
            InternalClassField = fields.First(f => f.Name.Equals("InternalField"));
            ProtectedClassField = fields.First(f => f.Name.Equals("ProtectedField"));

            // struct
            fields = typeof(FieldAccessorTestStruct).GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            PublicStructField = fields.First(f => f.Name.Equals("PublicField"));
            PrivateStructField = fields.First(f => f.Name.Equals("PrivateField"));
            InternalStructField = fields.First(f => f.Name.Equals("InternalField"));
            FakeLargeStructField = fields.First(f => f.Name.Equals("FakeLargeStructField"));
        }

        #region Class
        [Benchmark]
        public void PublicfieldTestOnClass_Direct()
        {
            SetField(fatc, "bleh");
        }
        private static void SetField(FieldAccessorTestClass instance, string publicFieldValue) => instance.PublicField = publicFieldValue;

        [Benchmark]
        public void PublicfieldTestOnClass_Reflection()
        {
            PublicClassField.SetValue(fatc, "bleh");
        }


        [Benchmark]
        public void PrivatefieldTestOnClass_Reflection()
        {
            PrivateClassField.SetValue(fatc, "bleh");
        }
        #endregion

        #region Struct
        [Benchmark]
        public void PublicfieldTestOnStruct_Direct()
        {
            SetStructField(fats, "bleh");
        }
        private static void SetStructField(FieldAccessorTestStruct instance, string publicFieldValue) => instance.PublicField = publicFieldValue;
        [Benchmark]
        public void PublicfieldTestOnStruct_Reflection()
        {
            PublicStructField.SetValue(fats, "bleh");
        }


        [Benchmark]
        public void PrivatefieldTestOnStruct_Reflection()
        {
            PrivateStructField.SetValue(fats, "bleh");
        }


        [Benchmark]
        public void FakeLargefieldTestOnStruct_Reflection()
        {
            FakeLargeStructField.SetValue(fats, fls);
        }
        #endregion
    }
}
