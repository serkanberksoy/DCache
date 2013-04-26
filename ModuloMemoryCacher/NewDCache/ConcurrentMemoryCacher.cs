#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewDCache.Interface;

#endregion

namespace NewDCache
{
    internal class ConcurrentMemoryCacher<TK, TV> : IMemoryCacher<TK, TV>
    {
        protected ConcurrentDictionary<TK, TV> CachedItemList;

        public ConcurrentMemoryCacher(int initialCacheSize, int concurrencyLevel, IExpiringDictionary<TK, TV> expiringDictionary)
        {
            CachedItemList = new ConcurrentDictionary<TK, TV>(concurrencyLevel, initialCacheSize);

            ExpiringDictionary = expiringDictionary;
            expiringDictionary.BeforeItemsExpired += ExpiringDictionaryBeforeItemsExpired;
        }

        public IExpiringDictionary<TK, TV> ExpiringDictionary { get; set; }

        protected virtual void ExpiringDictionaryBeforeItemsExpired(object sender, ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventArgs e)
        {
            if (!ExpiringDictionary.IsCustomTimerMethod && e.ExpiredKeys.Any())
            {
                ConcurrentDictionary<TK, TV> expiredItems = new ConcurrentDictionary<TK, TV>();
                Parallel.ForEach(e.ExpiredKeys, item =>
                                                    {
                                                        TV outVal;
                                                        expiredItems.TryAdd(item, GetValue(item));
                                                        CachedItemList.TryRemove(item, out outVal);
                                                        ExpiringDictionary.Remove(item);
                                                    });

                ExpiringDictionary.OnItemsExpired(
                    new ExpiringDictionary<TK, TV>.ItemsExpiredEventArgs(expiredItems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            }
        }

        #region Public Methods

        public List<TV> GetValues(List<TK> keys)
        {
            int keyCount = keys.Count;
            List<TV> result = new List<TV>(keyCount);

            Parallel.ForEach(keys, item => result.Add(GetValue(item)));

            return result;
        }

        public TV[] GetValues(TK[] keys)
        {
            return GetValues(keys.ToList()).ToArray();
        }

        public Dictionary<TK, TV> GetPairs(List<TK> keys)
        {
            Dictionary<TK, TV> result = new Dictionary<TK, TV>(keys.Count());

            Parallel.ForEach(keys, item => { result[item] = GetValue(item); });

            return result;
        }

        public TV GetValue(TK key)
        {
            TV result = default(TV);

            if (CachedItemList.ContainsKey(key))
            {
                result = CachedItemList[key];
            }

            ExpiringDictionary.SlideExpirationDate(key, true);

            return result;
        }

        public bool TryGetValue(TK key, out TV value)
        {
            bool result = default(bool);
            value = default(TV);

            TV temp = default(TV);
            if (CachedItemList.ContainsKey(key))
            {
                result = true;
                temp = CachedItemList[key];
            }


            ExpiringDictionary.SlideExpirationDate(key, true);

            value = temp;
            return result;
        }

        public void Set(TK key, TV value)
        {
            CachedItemList[key] = value;

            ExpiringDictionary.SlideExpirationDate(key);
        }

        public bool ContainsKey(TK key)
        {
            return Contains(key);
        }

        public bool Contains(TK key)
        {
            bool result = false;

            if (CachedItemList.ContainsKey(key))
            {
                if (IsTimerEnabled)
                {
                    result = ExpiringDictionary.GetKeyExpired(key);
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            if (Contains(item.Key) && item.Value.Equals(GetValue(item.Key)))
            {
                return true;
            }

            return false;
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(TK key, TV value)
        {
            Set(key, value);
        }

        public void AddRange(Dictionary<TK, TV> values)
        {
            Parallel.ForEach(values, item => Add(item.Key, item.Value));
        }

        bool IDictionary<TK, TV>.Remove(TK key)
        {
            Remove(key);
            return true;
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            TV outVal;
            CachedItemList.TryRemove(item.Key, out outVal);

            return true;
        }

        public void AddOrUpdate(TK key, TV value)
        {
            Set(key, value);
        }

        public bool AddIfNotExists(TK key, TV value)
        {
            bool result = default(bool);

            if (!CachedItemList.ContainsKey(key))
            {
                Set(key, value);
                result = true;
            }

            return result;
        }

        public TV GetOrAdd(TK key, TV value)
        {
            TV result;

            if (!CachedItemList.ContainsKey(key))
            {
                Set(key, value);
            }

            result = CachedItemList[key];

            ExpiringDictionary.SlideExpirationDate(key, true);

            return result;
        }

        public void Clear()
        {
            CachedItemList.Clear();
            ExpiringDictionary.Clear();
        }

        public void Remove(TK key)
        {
            TV outVal;
            CachedItemList.TryRemove(key, out outVal);
            ExpiringDictionary.Remove(key);
        }

        #endregion

        #region public properties

        public int Count
        {
            get { return CachedItemList.Count; }
        }

        bool ICollection<KeyValuePair<TK, TV>>.IsReadOnly
        {
            get { return false; }
        }

        public TV this[TK key]
        {
            get { return GetValue(key); }
            set { Set(key, value); }
        }

        ICollection<TK> IDictionary<TK, TV>.Keys
        {
            get { return CachedItemList.Keys; }
        }

        ICollection<TV> IDictionary<TK, TV>.Values
        {
            get { return CachedItemList.Values; }
        }


        public TimeSpan DataExpireDuration
        {
            get { return ExpiringDictionary.DataExpireDuration; }
            set { ExpiringDictionary.DataExpireDuration = value; }
        }

        public bool IsTimerEnabled
        {
            get { return ExpiringDictionary.IsTimerEnabled; }
        }

        public bool IsSliding
        {
            get { return ExpiringDictionary.IsSliding; }
            set { ExpiringDictionary.IsSliding = value; }
        }

        public TimeSpan TimerInterval
        {
            get { return ExpiringDictionary.TimerInterval; }
            set { ExpiringDictionary.TimerInterval = value; }
        }

        #endregion

        #region Interface Methods Not Implemented

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator()
        {
            return CachedItemList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return CachedItemList.GetEnumerator();
        }

        #endregion
    }
}