using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Axis.Luna.Notify
{
    public interface INotifier : INotifyPropertyChanged
    {
        void notify([CallerMemberName] string propertyName = null);
        void notify(Expression<Func<object>> exp);
    }
}
