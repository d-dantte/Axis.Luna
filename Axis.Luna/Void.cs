namespace Axis.Luna
{

    namespace MetaTypes
    {
        public class @void
        {
            public static @void Instance = new @void();
            private @void() { }

            public override bool Equals(object obj) => obj is @void;
            public override int GetHashCode() => 0;
            public override string ToString() => "@void";
        }
    }

    public static class Void
    {
        public static MetaTypes.@void @void => MetaTypes.@void.Instance; 
    }
}
