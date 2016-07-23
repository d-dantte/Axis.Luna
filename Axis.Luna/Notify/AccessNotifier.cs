using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Axis.Luna.Extensions;
using static Axis.Luna.Extensions.ExceptionExtensions;
using static Axis.Luna.Extensions.NotifierExtensions;
using System.Text.RegularExpressions;

namespace Axis.Luna.Notify
{
    public class PropertyChainNotifier<Source>
    where Source: class, INotifyPropertyChanged
    {

        public string Path { get; private set; }
        private PathSegment _head { get; set; }

        public PropertyChainNotifier(Source source, Expression<Func<Source, object>> accessPath, Action<object, EventArgs> onChange)
        {
            ThrowNullArguments(() => source, () => accessPath, () => onChange);

            _head =  new PathSegment(ExtractPathSegments(accessPath));
            _head.ManageNotification(source);

            _head.NotifyFor(p => true, (s, e) =>
            {
                var ne = e.As<NotifiedEventArgs>();
                onChange.Invoke(s, e);
            });
        }

        public void StopNotification() => _head.StopNotification();

        private IEnumerable<string> ExtractPathSegments(Expression accessPath)
            => accessPath is LambdaExpression ?
               ExtractPathSegments(accessPath.As<LambdaExpression>().Body) :

               accessPath is UnaryExpression ?
               ExtractPathSegments(accessPath.As<UnaryExpression>().Operand) :

               accessPath.ToString()
                         .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                         .Skip(1)
                         .ToArray();


        public class PathSegment: NotifierBase
        {
            public string Path { get; internal set; } = null;
            private string[] _segments = null;
            private PathSegment _nextSegment = null;
            private NotifyRegistrar _registrar = null;
            private DelegatePropertySurrogate pps = null;
            private INotifyPropertyChanged _source = null;
            EqualityComparer<object> Equalizer = EqualityComparer<object>.Default;

            public PathSegment(IEnumerable<string> path)
            {
                pps = new DelegatePropertySurrogate(this);
                _segments = path.ToArray();
                Path = string.Join(".", _segments);
                if (_segments.Count() > 1)
                {
                    _nextSegment = new PathSegment(path.Skip(1));
                    _nextSegment.NotifyFor(_nextSegment.Path, (sender, arg) =>
                    {
                        var ne = arg.As<NotifiedEventArgs>();

                        //if a "xyz.notify()" call was made, with no actual change to the property...
                        if (Equalizer.Equals(ne.newValue, pps.Get<object>(Path))) pps.Notify(Path);
                        else pps.Set(Path, ne.newValue);
                    });
                }
            }

            internal void ManageNotification(INotifyPropertyChanged source)
            {
                _registrar = (_source = source)?.NotifyFor(_segments.First(), (sender, arg) =>
                {
                    var ne = arg.As<NotifiedEventArgs>();
                    if(ne.oldValue != null) _nextSegment?.StopNotification();

                    //manage notification if possible (if there is a next segment, and @new is not null
                    _nextSegment?.ManageNotification(ne.newValue.As<INotifyPropertyChanged>());

                    //if a "xyz.notify()" call was made, with no actual change to the property...
                    var sv = SourceValue();
                    if (Equalizer.Equals(sv, pps.Get<object>(Path))) pps.Notify(Path);
                    else pps.Set(Path, sv);
                });

                source?.PropertyValue(_segments.First())
                       .As<INotifyPropertyChanged>()
                       .PipeIf(v => v != null && _nextSegment != null, v => _nextSegment.ManageNotification(v));
            }

            private object SourceValue()
                => _nextSegment != null ? _nextSegment.SourceValue() : _source?.PropertyValue(Path);

            public void StopNotification()
            {
                _nextSegment?.StopNotification();
                _registrar?.Unregister();
                _registrar = null;
            }
        }
    }
}
