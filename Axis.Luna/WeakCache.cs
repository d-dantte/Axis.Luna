using Axis.Luna.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Axis.Luna
{
    public class WeakCache
    {
        private ConcurrentDictionary<string, CachePayload> _cache = new ConcurrentDictionary<string, CachePayload>();

        public Data GetOrAdd<Data>(string cacheKey, Func<string, Data> dataProvider)
        where Data: class
        {
            var payload = _cache.GetOrAdd(cacheKey, _key => new CachePayload<Data>(cacheKey, dataProvider)).As<CachePayload<Data>>();

            Data _data;
            if (payload.Ref.TryGetTarget(out _data)) return _data;
            else return payload.Refresh();
        }

        public WeakCache Invalidate(string cacheKey)
        {
            CachePayload cp;
            if(_cache.TryGetValue(cacheKey, out cp)) cp.AsDynamic().SetTarget(null);

            return this;
        }
        
        public WeakCache InvalidateAll()
        {
            _cache.Keys
                  .ToArray()
                  .ForAll((_cnt, _next) => Invalidate(_next));

            return this;
        }
    }

    internal class CachePayload
    {
        internal readonly object SyncLock = new object();
        internal string CacheKey { get; set; }
    }

    internal class CachePayload<Data>: CachePayload
    where Data: class
    {
        internal WeakReference<Data> Ref { get; private set; }
        internal Func<string, Data> DataProvider { get; private set; }


        internal CachePayload(string key, Func<string, Data> dataProvider)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new Exception();

            DataProvider = dataProvider;
            CacheKey = key;
            Ref = new WeakReference<Data>(DataProvider.Invoke(key));
        }

        internal Data Refresh()
        {
            Data _data;
            lock (SyncLock)
            {
                if (!Ref.TryGetTarget(out _data))
                    Ref.SetTarget(_data = DataProvider.Invoke(CacheKey));
            }

            return _data;
        }
    }
}
