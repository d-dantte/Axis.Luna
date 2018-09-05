using Axis.Luna.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Axis.Luna
{
    public class WeakCache
    {
        private ConcurrentDictionary<string, CachePayload> _cache = new ConcurrentDictionary<string, CachePayload>();

        /// <summary>
        /// This code guarantees that dataProvider will be called only once
        /// </summary>
        /// <typeparam name="Data"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public Data GetOrAdd<Data>(string cacheKey, Func<string, Data> dataProvider)
        where Data: class
        {
            var payload = _cache.GetOrAdd(cacheKey, _key => new CachePayload<Data>(_key, dataProvider))
                                .Cast<CachePayload<Data>>();
            
            return payload.GetOrRefresh();
        }

        public Data GetOrRefresh<Data>(string cacheKey)
        where Data : class
        {
            CachePayload _pl;
            if (!_cache.TryGetValue(cacheKey, out _pl)) return null;

            else return _pl.Cast<CachePayload<Data>>()?.GetOrRefresh();
        }

        public Data Get<Data>(string cacheKey)
        where Data : class
        {
            CachePayload _pl;
            if (!_cache.TryGetValue(cacheKey, out _pl)) return null;

            Data _data;
            if (!_pl.Cast<CachePayload<Data>>().Ref.TryGetTarget(out _data)) return null;
            else return _data;
        }

        public WeakCache Invalidate(string cacheKey)
        {
            CachePayload cp;
            if(_cache.TryGetValue(cacheKey, out cp)) cp.AsDynamic().Ref.SetTarget(null);

            return this;
        }
        public WeakCache InvalidateAll()
        {
            _cache.Keys
                  .ToArray() //<-- get a snapshot of the keys
                  .ForAll(_next => Invalidate(_next));

            return this;
        }

        public Data Refresh<Data>(string cacheKey)
        where Data: class
        {
            CachePayload _pl;
            if (!_cache.TryGetValue(cacheKey, out _pl))
                throw new Exception("The key is not contained in the cache");

            else
                return _pl.Cast<CachePayload<Data>>().GetOrRefresh(true);
        }        
    }

    internal abstract class CachePayload
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
        }


        internal Data GetOrRefresh(bool forceRefresh = false)
        {
            Data _data;
            lock (SyncLock)
            {
                if (Ref == null)//called first time only
                    Ref = new WeakReference<Data>(_data = DataProvider.Invoke(CacheKey));

                else if (forceRefresh || !Ref.TryGetTarget(out _data))
                    Ref.SetTarget(_data = DataProvider.Invoke(CacheKey));
            }

            return _data;
        }
    }
}
