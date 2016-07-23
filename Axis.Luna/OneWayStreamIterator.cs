using static Axis.Luna.Extensions.ObjectExtensions;
using static Axis.Luna.Extensions.ExceptionExtensions;

using Axis.Luna;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Axis.Luna
{
    public class OneWayStreamIterator<StreamImpl, Item> : IEnumerable<Item>, IEnumerator<Item>
    where StreamImpl : class
    {
        public delegate bool Popper(StreamImpl stream, out Item item);
        public delegate bool Pusher(StreamImpl stream, Item item);

        private StreamImpl _stream = null;
        private Pusher _pusher = null;
        private Popper _tryPop = null;
        //private AutoResetEvent _pushSignaller = new AutoResetEvent(false);
        private Pulser _pushSignaller = new Pulser();

        public OneWayStreamIterator(StreamImpl queue, Pusher pusher, Popper popper)
        {
            ThrowNullArguments(() => queue, () => pusher, () => popper);

            _stream = queue;
            _pusher = pusher;
            _tryPop = popper;
        }

        public bool Push(Item item)
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(OneWayStreamIterator<StreamImpl, Item>));

            else if (_pusher.Invoke(_stream, item))
                return _pushSignaller.PulseOne().Pipe(t => true);
            else
                return false;
        }

        #region IEnumerable
        public IEnumerator<Item> GetEnumerator()
            => new InternalEnumerator(this).ThrowIf(t => IsDisposed, "object is disposed");

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion

        #region IEnumerator
        private Item _current;

        public Item Current => _current.ThrowIf(t => IsDisposed, "object is disposed");

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(OneWayStreamIterator<StreamImpl, Item>));

            Item temp;
            while (true)
            {
                if (IsDisposed) return false;
                else if (_tryPop(_stream, out temp)) return (_current = temp).Pipe(t => true);
                else _pushSignaller.Wait();
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IDisposable
        protected bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
                _pushSignaller.PulseAll(); //release whoever was waiting on the enumerator
            }
        }
        #endregion

        internal class InternalEnumerator : IEnumerator<Item>
        {
            private OneWayStreamIterator<StreamImpl, Item> Owner = null;
            internal InternalEnumerator(OneWayStreamIterator<StreamImpl, Item> owner)
            {
                this.Owner = owner;
            }

            public Item Current => Owner.Current;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                Owner = null;
            }

            public bool MoveNext() => Owner.MoveNext();

            public void Reset()
            {
            }
        }
    }
}
