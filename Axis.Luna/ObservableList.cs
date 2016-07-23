using static Axis.Luna.Extensions.DelegateMediatorExtensions;

using Axis.Luna.Extensions;
using Axis.Luna.Notify;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Axis.Luna
{
    public class ObservableList<T> : NotifierBase, IList<T>, INotifyCollectionChanged
    {
        public ObservableList() : this(new List<T>())
        { }
        public ObservableList(IEnumerable<T> collection)
        {
            this.internalList = new List<T>(collection);
            this.IsNotificationEnabled = true;
        }

        public ObservableList(int capacity) : this(new List<T>(capacity))
        { }

        private List<T> internalList { get; set; }

        public bool IsNotificationEnabled { get; set; }


        private void notifyCollection(NotifyCollectionChangedEventArgs args)
        {
            if (_collectionChanged == null) return;

            #region old logic
            ///Create a DispaterAwareObservableList in the Libra project, and ship off the "Dispatcher logic in there, since it depends on the Windowbase.dll assembly

            //foreach (NotifyCollectionChangedEventHandler handler in _collectionChanged.GetInvocationList())
            //foreach (EventHandler<NotifyCollectionChangedEventArgs> handler in _collectionChanged.GetInvocationList())
            //{
            //    var dispatcherObject = handler.TrueTarget() as DispatcherObject;
            //    //var dispatcherObject = handler.Target as DispatcherObject;

            //    if (dispatcherObject != null && !dispatcherObject.CheckAccess())
            //        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, args);

            //    else handler(this, args); // note : this does not execute handler in target thread's context
            //}
            #endregion
            
            _collectionChanged.Invoke(this, args);
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return this.internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.internalList.Insert(index, item);

            if (IsNotificationEnabled)
            {
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                this.notify(() => this.Count);
            }
        }

        public void RemoveAt(int index)
        {
            var olditem = this[index];
            this.internalList.RemoveAt(index);

            if (IsNotificationEnabled)
            {
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, olditem, index));
                this.notify(() => this.Count);
            }
        }

        public T this[int index]
        {
            get { return this.internalList[index]; }
            set
            {
                var old = this.internalList[index];
                this.internalList[index] = value;

                if (this.IsNotificationEnabled)
                {
                    this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                                                               value.Enumerate().ToList(),
                                                                               old.Enumerate().ToList()));
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            this.internalList.Add(item);

            if (IsNotificationEnabled)
            {
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                this.notify(() => this.Count);
            }
        }

        public void Clear()
        {
            var oldItems = new List<T>(this.internalList);
            this.internalList.Clear();

            if (IsNotificationEnabled)
            {
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, 0));
                this.notify(() => this.Count);
            }
        }

        public bool Contains(T item)
        {
            return internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.internalList.CopyTo(array, arrayIndex);
        }

        public int Count { get { return internalList.Count; } }

        public bool IsReadOnly { get { return false; } }

        public bool Remove(T item)
        {
            var indx = IndexOf(item);
            if (indx < 0 || indx >= this.Count) return false;

            RemoveAt(indx);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        private NotifyCollectionChangedEventHandler _collectionChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChanged += ManagedCallback(value, del => _collectionChanged -= del); }
            remove { RemoveManagedCallback(_collectionChanged, value); }
        }

        #endregion


        #region List Parallels

        public int BinarySearch(T item)
        {
            return internalList.BinarySearch(item);
        }
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return internalList.BinarySearch(item, comparer);
        }
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return internalList.BinarySearch(index, count, item, comparer);
        }


        public bool Exists(Predicate<T> match)
        {
            return internalList.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return internalList.Find(match);
        }
        public List<T> FindAll(Predicate<T> match)
        {
            return internalList.FindAll(match);
        }
        public int FindIndex(Predicate<T> match)
        {
            return internalList.FindIndex(match);
        }
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return internalList.FindIndex(startIndex, match);
        }
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            return internalList.FindIndex(startIndex, count, match);
        }
        public T FindLast(Predicate<T> match)
        {
            return internalList.FindLast(match);
        }
        public int FindLastIndex(Predicate<T> match)
        {
            return internalList.FindLastIndex(match);
        }
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return internalList.FindLastIndex(startIndex, match);
        }
        public void ForEach(Action<T> action)
        {
            internalList.ForEach(action);
        }
        public void ForEach(Action<int, T> action)
        {
            for (int cnt = 0; cnt < this.internalList.Count; cnt++) action(cnt, this[cnt]);
        }
        public List<T> GetRange(int index, int count)
        {
            return internalList.GetRange(index, count);
        }
        public int IndexOf(T item, int index)
        {
            return internalList.IndexOf(item, index);
        }
        public int IndexOf(T item, int index, int count)
        {
            return internalList.IndexOf(item, index, count);
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var oldCount = this.Count;
            var shiftCount = oldCount - index;
            internalList.InsertRange(index, collection);
            if (IsNotificationEnabled)
            {
                var diff = this.Count - oldCount;
                //range to be shifted
                var shiftedRange = internalList.GetRange(index + diff, shiftCount);

                //add event
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                           collection.ToList()));

                //move event
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,
                                                                           shiftedRange,
                                                                           index + diff,
                                                                           index));

                this.notify(() => this.Count);
            }

        }
        public int LastIndexOf(T item)
        {
            return internalList.LastIndexOf(item);
        }
        public int LastIndexOf(T item, int index)
        {
            return internalList.LastIndexOf(item, index);
        }
        public int LastIndexOf(T item, int index, int count)
        {
            return internalList.LastIndexOf(item, index, count);
        }
        public int RemoveAll(Predicate<T> match)
        {
            var list = new List<int>();
            for (int cnt = this.Count - 1; cnt >= 0; cnt--)
            {
                if (match(this[cnt])) list.Add(cnt);
            }

            var removed = 0;
            list.ForEach(index =>
            {
                this.RemoveAt(index);
                removed++;
            });

            return removed;
        }
        public void RemoveRange(int index, int count)
        {
            var range = internalList.GetRange(index, count);
            internalList.RemoveRange(index, count);

            if (IsNotificationEnabled)
            {
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, range));
                this.notify(() => this.Count);
            }
        }
        public void Reverse(int index, int count)
        {
            internalList.Reverse(index, count);

            if (IsNotificationEnabled)
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void Sort()
        {
            internalList.Sort();

            if (IsNotificationEnabled)
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void Sort(Comparison<T> comparison)
        {
            internalList.Sort(comparison);

            if (IsNotificationEnabled)
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public void Sort(IComparer<T> comparer)
        {
            internalList.Sort(comparer);

            if (IsNotificationEnabled)
                this.notifyCollection(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        public T[] ToArray()
        {
            return internalList.ToArray();
        }
        public void TrimExcess()
        {
            internalList.TrimExcess();
        }
        public bool TrueForAll(Predicate<T> match)
        {
            return internalList.TrueForAll(match);
        }
        #endregion
    }
}
