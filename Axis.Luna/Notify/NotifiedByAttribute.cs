using System;
using System.Collections.Generic;

namespace Axis.Luna.Notify
{

    ///TODO: implement object graph notification. ie <code>[Notifies("customer.age")]</code> The previous statement will locate a "customer"
    ///property of the current object, check that it is an "INotifier" instance, then go ahead and notify its "age" property if it exists.
    [AttributeUsage(AttributeTargets.Property)]
    public class NotifiedByAttribute : Attribute
    {
        public IEnumerable<string> targets
        {
            get { return propNames == null ? new string[0] : this.propNames; }
        }
        public bool inheritBaseNotifiables { get; private set; }

        private string[] propNames = null;

        public NotifiedByAttribute(params string[] propertyNames) : this(false, propertyNames)
        { }

        public NotifiedByAttribute(bool inherit, params string[] propertyNames)
        {
            this.propNames = propertyNames;
            this.inheritBaseNotifiables = inherit;
        }
    }
}
