using System.Text;

namespace Axis.Luna.Unions
{
//    internal readonly struct TypeArg
//    {
//        private readonly object _type;

//        public bool IsGeneric => _type is string;

//        public bool IsConcrete => _type is Type;

//        public TypeArg(Type type)
//        {
//            _type = type ?? throw new ArgumentNullException(nameof(type));
//        }

//        public TypeArg(string genericType)
//        {
//            if (string.IsNullOrWhiteSpace(genericType))
//                throw new ArgumentNullException(nameof(genericType));

//            _type = genericType;
//        }

//        public bool Is(out Type? value)
//        {
//            if (_type is Type t)
//            {
//                value = t;
//                return true;
//            }

//            value = null;
//            return false;
//        }

//        public bool Is(out string? value)
//        {
//            if (_type is string t)
//            {
//                value = t;
//                return true;
//            }

//            value = null;
//            return false;
//        }

//        public override string ToString() => _type?.ToString()!;

//        public string TypeDeclaration() => _type switch
//        {
//            string s => s,
//            Type t => TypeDeclaration(t),
//            _ => throw new InvalidOperationException(
//                $"Invalid type: null/unknown")
//        };

//        public static implicit operator TypeArg(Type param) => new(param);
//        public static implicit operator TypeArg(string param) => new(param);

//        internal static string TypeDeclaration(Type type)
//        {
//            var sbuilder = new StringBuilder(type.Name);
//            if (type.IsGenericType)
//            {
//                sbuilder.Append('<');
//                var gargs = type.GetGenericArguments();
//                for (int cnt = 0; cnt < gargs.Length; cnt++)
//                {
//                    if (cnt > 0)
//                        sbuilder.Append(", ");
//                    sbuilder = sbuilder.Append(TypeDeclaration(gargs[cnt]));
//                }
//                sbuilder = sbuilder.Append('>');
//            }
//            return sbuilder.ToString();
//        }
//    }
}
