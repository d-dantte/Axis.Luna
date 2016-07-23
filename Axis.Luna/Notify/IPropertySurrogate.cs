using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Notify
{
    public interface IPropertySurrogate
    {
        NotifierBase Target { get; }

        string ResolvePropertyName(string unresolvedPropertyName);

        void Set(string unresolvedPropertyName, object value);
        void Set(Expression<Func<object>> exp, object value);

        V Get<V>(string unresolvedPropertyName);
        V Get<V>(Expression<Func<object>> exp);
    }
}
