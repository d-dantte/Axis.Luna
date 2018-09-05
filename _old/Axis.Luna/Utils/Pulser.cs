using System.Collections.Concurrent;
using System.Threading;

namespace Axis.Luna
{
    public class Pulser
    {
        private AutoResetEvent _are = new AutoResetEvent(false);
        private ConcurrentDictionary<string, string> _waitQueue = new ConcurrentDictionary<string, string>();

        private string currentThreadId() => $"{Thread.CurrentThread.ManagedThreadId}[{Thread.CurrentThread.GetHashCode()}]";
        public void Wait()
        {
            var id = currentThreadId();
            try
            {
                _waitQueue.GetOrAdd(id, id);
                _are.WaitOne();
            }
            finally
            {
                string temp = null;
                _waitQueue.TryRemove(id, out temp);
            }
        }

        public bool PulseOne() => _are.Set();

        public bool PulseAll()
        {
            var threads = _waitQueue.Keys.Count;
            while (threads-- > 0) _are.Set();

            return true;
        }
    }
}
