using static Axis.Luna.Extensions.EnumerableExtensions;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.ExceptionExtensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Axis.Luna
{
    public abstract class StructuredEnum
    {
        private static Dictionary<Type, List<StructuredEnum>> Enums = new Dictionary<Type, List<StructuredEnum>>();

        protected StructuredEnum()
        { }

        /// <summary>
        /// Loops through all static readonly FIELDS for unassigned fields, and generates a value for them
        /// </summary>
        protected static void SynthesizeValues<Enum>(Func<string, Enum> generator)
        where Enum : StructuredEnum
        {
            var type = typeof(Enum);
            type.GetRuntimeFields()
                .Where(f => f.IsStatic && f.IsInitOnly && f.FieldType == type)
                .Select(f => new { field = f, @enum = generator.Invoke(f.Name).ThrowIfNull() })
                .UsingEach(n => n.field.SetValue(null, n.@enum))
                .ForAll((cnt, next) => Enums.GetOrAdd(type, t => new List<StructuredEnum>()).Add(next.@enum));
        }

        public static IEnumerable<Enm> Values<Enm>()
        where Enm : StructuredEnum
        {
            var te = typeof(Enm);
            if (!Enums.ContainsKey(te)) ThruttleConstruction(te);
            return Eval(() => Enums[te].Cast<Enm>().ToArray(), ex => new Enm[0]);
        }
        
        /// <summary>
        /// access a static member so the static constructor is called.
        /// </summary>
        private static void ThruttleConstruction(Type t)
        {
            t.GetRuntimeFields()
             .FirstOrDefault(f => f.IsStatic && f.IsInitOnly && f.FieldType == t)?
             .GetValue(null);
        }
    }
}
