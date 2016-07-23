using static Axis.Luna.Extensions.ObjectExtensions;

using Axis.Luna.Extensions;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace Axis.Luna.Notify
{
    public class NotifiedBinding
    {
        private static readonly string CallContextTag = "$__NBindable__Axis.Luna.NotifiedBinding";
        public enum Mode { TwoWay, LeftToRight, RightToLeft }

        public NotifiedBinding(BindingProfile left, BindingProfile right, Mode mode = Mode.TwoWay)
        {
            //validate the property to be bound
            if (left.property.PropertyType != right.property.PropertyType) throw new ArgumentException();

            this.mode = mode;

            this.left = left;
            if (mode == Mode.TwoWay || mode == Mode.LeftToRight)
                this.left.notifiable.PropertyChanged += leftChanged;

            this.right = right;
            if (mode == Mode.TwoWay || mode == Mode.RightToLeft)
                this.right.notifiable.PropertyChanged += rightChanged;
        }

        #region Left
        public BindingProfile left { get; private set; }
        private void leftChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == left.property.Name)
            {
                var data = CallContext.LogicalGetData(CallContextTag);
                if (data != null) return;

                //set the call context guard
                CallContext.LogicalSetData(CallContextTag, CallContextTag);

                //set the value on the right hand side
                right.set(left.get());

                //remove the call context guard
                CallContext.LogicalSetData(CallContextTag, null);
            }
        }
        #endregion

        #region Right
        public BindingProfile right { get; private set; }
        private void rightChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == right.property.Name)
            {
                var data = CallContext.LogicalGetData(CallContextTag);
                if (data != null) return;

                //set the call context guard
                CallContext.LogicalSetData(CallContextTag, CallContextTag);

                //set the value on the left hand side
                left.set(right.get());

                //remove the call context guard
                CallContext.LogicalSetData(CallContextTag, null);
            }
        }
        #endregion

        public Mode mode { get; private set; }

        public void release()
        {
            Eval(() => left.notifiable.PropertyChanged -= leftChanged);
            Eval(() => right.notifiable.PropertyChanged -= rightChanged);

            left = null;
            right = null;
        }



        public class BindingProfile
        {
            public INotifier notifiable { get; private set; }
            public PropertyInfo property { get; private set; }

            public BindingProfile(INotifier obj, string prop)
            {
                if (obj == null || prop == null) throw new ArgumentNullException();

                notifiable = obj;
                property = obj.Property(prop);
            }

            public void set(object value)
                => property.SetValue(notifiable, value);
            public object get()
                => property.GetValue(notifiable);
        }
    }
}
