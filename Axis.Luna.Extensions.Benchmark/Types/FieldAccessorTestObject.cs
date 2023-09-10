namespace Axis.Luna.Extensions.Benchmark.Types
{
    internal class FieldAccessorTestClass
    {
        public string PublicField;

        private string PrivateField;

        protected string ProtectedField;

        internal string InternalField;
    }

    internal struct FieldAccessorTestStruct
    {
        public string PublicField;

        private string PrivateField;

        internal string InternalField;

        private FakeLargeStruct FakeLargeStructField;
    }

    internal struct FakeLargeStruct
    {
        public Decimal d1;
        public Decimal d2;
        public Decimal d3;
        public Decimal d4;
        public Decimal d5;
        public Decimal d6;
        public Decimal d7;
        public Decimal d8;
        public Decimal d9;
        public Decimal d10;
        public Decimal d11;
        public Decimal d12;
        public Decimal d13;
        public Decimal d14;
        public Decimal d15;
        public Decimal d16;
        public Decimal d17;
        public Decimal d18;
        public Decimal d19;
    }
}
