using static Axis.Luna.Extensions.DelegateMediatorExtensions;
using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.TypeExtensions;
using static Axis.Luna.Extensions.ExceptionExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Axis.Luna.Extensions;

namespace Axis.Luna.Notify
{

    public abstract class NotifierBase : INotifier
    {
        #region INotifyPropertyChanged Members
        //private EventHandler<PropertyChangedEventArgs> _propChanged; //added
        private PropertyChangedEventHandler _propChanged; //added
        public event PropertyChangedEventHandler PropertyChanged
        #region old implementation
        //{
        //    add
        //    {
        //        this._propChanged +=
        //            new EventHandler<PropertyChangedEventArgs>(value).MakeWeak(eh => _propChanged -= eh);
        //    }
        //    remove { new EventHandler<PropertyChangedEventArgs>(value).RemoveFrom(ref _propChanged); }
        //}
        #endregion
        {
            add
            {
                _propChanged += value.IsManaged() ? value : ManagedCallback(value, del => _propChanged -= del);
            }
            remove { _propChanged = RemoveManagedCallback(_propChanged, value); }
        }
        #endregion

        #region Properties
        internal IEnumerable<string> Availableproperties => values.Keys;
        #endregion

        #region Fields
        private Dictionary<string, object> values = new Dictionary<string, object>();
        private Dictionary<string, object> oldvalues = new Dictionary<string, object>();
        internal static ConcurrentDictionary<Type, Dictionary<string, HashSet<string>>> propertyDependency =
                          new ConcurrentDictionary<Type, Dictionary<string, HashSet<string>>>();
        #endregion

        #region init
        protected NotifierBase()
        {
        }
        #endregion

        #region Handlers
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var ne = (e as NotifiedEventArgs) ??
                     new NotifiedEventArgs(e.PropertyName, sender.PropertyValue(e.PropertyName), getOld(e.PropertyName));

            if (ne.notified.Contains(ne.PropertyName)) return;
            else ne.notified.Add(ne.PropertyName);

            _propChanged?.Invoke(this, ne);
            //if (this.PropertyChanged != null) this.PropertyChanged(this, ne);
            this.notifyTargets(e.PropertyName, ne.notified);
        }
        #endregion

        #region Utils
        public void notify(Expression<Func<object>> exp)
        {
            var lambda = exp as LambdaExpression;
            if (lambda == null) return;
            else if (lambda.Body is UnaryExpression)
            {
                var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                if (member == null) return;
                else notify(member.Member.Name);
            }
            else if (lambda.Body is MemberExpression)
            {
                var member = (lambda.Body as MemberExpression);
                notify(member.Member.Name);
            }
        }
        public void notify([CallerMemberName] string propertyName = null)
        {
            notify(propertyName, new HashSet<string>());
        }
        private void notify(string propertyName, ISet<string> notified)
        {
            var ne = new NotifiedEventArgs(propertyName, get<object>(propertyName), getOld(propertyName), notified);
            this.OnPropertyChanged(this, ne);
        }
        private void notifyTargets(string pname, ISet<string> notified = null)
        {
            var type = this.GetType();
            var prop = type.GetProperty(pname);
            if (prop == null) return;

            var notifies = prop.Notifiables();
            notifies.ToList().ForEach(pn => notify(pn, notified));
        }

        protected V get<V>([CallerMemberName] string property = null)
        {
            if (values.ContainsKey(property))
            {
                var v = (V)values[property];
                if (v == null) return default(V);
                else return v;
            }
            else return default(V);
        }
        private object getOld(string property)
        {
            if (!oldvalues.ContainsKey(property)) return null;
            else return oldvalues[property];
        }

        protected void set<V>(ref V value, [CallerMemberName] string property = null)
        {
            if (!values.ContainsKey(property))
            {
                oldvalues[property] = null;
                values[property] = value;
                notify(property);
            }

            //only modify if the old and new values are different
            else if (!EqualityComparer<V>.Default.Equals(value, (V)values[property]))
            {
                oldvalues[property] = values[property];
                values[property] = value;
                notify(property);
            }
        }
        protected bool isSet([CallerMemberName] string property = null) => values.ContainsKey(property ?? "");
        #endregion


        
        /// <summary>
        /// Used for setting prefixed-attached properties on the <c>NotifierBase</c>.
        /// Attached properties are properties that do not naturally occur on a "Notifiable" object.
        /// If a key is specified, the key's hashcode is used to generate a unique property name for the property.
        /// This makes it further possible to have multiple properties set with the same base property name, but
        /// differentiated by the keys used.
        /// </summary>
        public class PrefixedPropertySurrogate: IPropertySurrogate
        {
            #region Statics
            private static readonly string DefaultPrefix = "_____PrefixedProperty_";
            #endregion

            #region init
            public PrefixedPropertySurrogate(NotifierBase targetObject, string prefix = null)
            {
                ThrowNullArguments(() => targetObject);

                this.Prefix = prefix?.Trim().ThrowIf(pfx => string.Empty.Equals(pfx), "Invalid Prefix") ?? DefaultPrefix;
                this.Target = targetObject;
            }
            #endregion

            #region properties
            public string Prefix { get; set; }
            public NotifierBase Target { get; private set; }
            #endregion

            #region methods
            public string ResolvePropertyName(string property)
                => $"{Prefix}_{property.ThrowIf(p => string.IsNullOrWhiteSpace(p), "Invalid Property Name").Trim()}";
            public void Set(string unresolvedPropertyName, object value) => Target.set(ref value, ResolvePropertyName(unresolvedPropertyName));
            public V Get<V>(string unresolvedPropertyName) => Target.get<V>(ResolvePropertyName(unresolvedPropertyName)).As<V>();

            public void Set(Expression<Func<object>> exp, object value)
            {
                var lambda = exp as LambdaExpression;
                if (lambda == null) return;
                else if (lambda.Body is UnaryExpression)
                {
                    var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                    if (member == null) return;
                    else Set(member.Member.Name, value);
                }
                else if (lambda.Body is MemberExpression)
                {
                    var member = (lambda.Body as MemberExpression);
                    Set(member.Member.Name, value);
                }
            }
            public V Get<V>(Expression<Func<object>> exp)
            {
                var lambda = exp as LambdaExpression;
                if (lambda == null) return default(V);
                else if (lambda.Body is UnaryExpression)
                {
                    var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                    if (member == null) return default(V);
                    else return Get<V>(member.Member.Name);
                }
                else if (lambda.Body is MemberExpression)
                {
                    var member = (lambda.Body as MemberExpression);
                    return Get<V>(member.Member.Name);
                }
                else return default(V);
            }

            public bool IsPropertyAttached(string unresolvedPropertyName) =>  Target.isSet(ResolvePropertyName(unresolvedPropertyName));
            #endregion
        }

        /// <summary>
        /// This Class does not prefix it's property names; the implication of this is that properties set via this class may clash with 
        /// naturally occuring properties on the target itself - which is the desired effect of this
        /// </summary>
        public class DelegatePropertySurrogate: IPropertySurrogate
        {
            #region init
            public DelegatePropertySurrogate(NotifierBase targetObject)
            {
                ThrowNullArguments(() => targetObject);
                
                Target = targetObject;
            }
            #endregion

            #region properties
            public NotifierBase Target { get; private set; }
            public IEnumerable<string> Properties => Target.Availableproperties ?? new string[0];
            #endregion

            #region methods
            public string ResolvePropertyName(string property)
                =>  property.ThrowIf(p => string.IsNullOrWhiteSpace(p), "Invalid Property Name").Trim();
            public void Set(string unresolvedPropertyName, object value) => Target.set(ref value, ResolvePropertyName(unresolvedPropertyName));
            public V Get<V>(string unresolvedPropertyName) => Target.get<V>(ResolvePropertyName(unresolvedPropertyName)).As<V>();

            public void Set(Expression<Func<object>> exp, object value)
            {
                var lambda = exp as LambdaExpression;
                if (lambda == null) return;
                else if (lambda.Body is UnaryExpression)
                {
                    var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                    if (member == null) return;
                    else Set(member.Member.Name, value);
                }
                else if (lambda.Body is MemberExpression)
                {
                    var member = (lambda.Body as MemberExpression);
                    Set(member.Member.Name, value);
                }
            }
            public V Get<V>(Expression<Func<object>> exp)
            {
                var lambda = exp as LambdaExpression;
                if (lambda == null) return default(V);
                else if (lambda.Body is UnaryExpression)
                {
                    var member = (lambda.Body as UnaryExpression).Operand as MemberExpression;
                    if (member == null) return default(V);
                    else return Get<V>(member.Member.Name);
                }
                else if (lambda.Body is MemberExpression)
                {
                    var member = (lambda.Body as MemberExpression);
                    return Get<V>(member.Member.Name);
                }
                else return default(V);
            }

            public bool IsPropertyAttached(string unresolvedPropertyName) => Target.isSet(ResolvePropertyName(unresolvedPropertyName));

            public void Notify(string unresolvedName) => Target.notify(ResolvePropertyName(unresolvedName));
            #endregion
        }
    }
}
