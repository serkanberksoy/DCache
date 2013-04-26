#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NewDCache.Interface;

#endregion

namespace NewDCache
{
    internal class MemoryCacher<TK, TV> : IMemoryCacher<TK, TV>
    {
        protected ReaderWriterLockSlim AddRemoveLock;
        protected Dictionary<TK, TV> CachedItemList;

        public MemoryCacher(int initialCacheSize, IExpiringDictionary<TK, TV> expiringDictionary)
        {
            AddRemoveLock = new ReaderWriterLockSlim();
            CachedItemList = new Dictionary<TK, TV>(initialCacheSize);

            ExpiringDictionary = expiringDictionary;
            expiringDictionary.BeforeItemsExpired += ExpiringDictionaryBeforeItemsExpired;
        }

        public IExpiringDictionary<TK, TV> ExpiringDictionary { get; set; }

        protected virtual void ExpiringDictionaryBeforeItemsExpired(object sender, ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventArgs e)
        {
            if (!ExpiringDictionary.IsCustomTimerMethod && e.ExpiredKeys.Any())
            {
                Dictionary<TK, TV> expiredItems = new Dictionary<TK, TV>();

                AddRemoveLock.EnterUpgradeableReadLock();

                e.ExpiredKeys.ForEach(item =>
                                          {
                                              if (CachedItemList.ContainsKey(item))
                                              {
                                                  expiredItems.Add(item, CachedItemList[item]);
                                              }
                                          });

                AddRemoveLock.EnterWriteLock();

                e.ExpiredKeys.ForEach(item =>
                                          {
                                              TV outVal;
                                              CachedItemList.Remove(item);
                                              ExpiringDictionary.Remove(item);
                                          });
                AddRemoveLock.ExitWriteLock();
                AddRemoveLock.ExitUpgradeableReadLock();

                ExpiringDictionary.OnItemsExpired(
                    new ExpiringDictionary<TK, TV>.ItemsExpiredEventArgs(expiredItems.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
            }
        }

        #region Public Methods

        public List<TV> GetValues(List<TK> keys)
        {
            int keyCount = keys.Count;
            List<TV> result = new List<TV>(keyCount);

            for (int i = 0; i < keyCount; ++i)
            {
                result.Add(GetValue(keys[i]));
            }

            return result;
        }

        public TV[] GetValues(TK[] keys)
        {
            int keyCount = keys.Length;
            TV[] result = new TV[keyCount];

            for (int i = 0; i < keyCount; ++i)
            {
                result[i] = GetValue(keys[i]);
            }

            return result;
        }

        public Dictionary<TK, TV> GetPairs(List<TK> keys)
        {
            Dictionary<TK, TV> result = new Dictionary<TK, TV>();

            int keyCount = keys.Count;
            for (int i = 0; i < keyCount; ++i)
            {
                result[keys[i]] = GetValue(keys[i]);
            }

            return result;
        }

        public TV GetValue(TK key)
        {
            TV result = default(TV);

            AddRemoveLock.EnterUpgradeableReadLock();

            if (CachedItemList.ContainsKey(key))
            {
                result = CachedItemList[key];
            }

            AddRemoveLock.ExitUpgradeableReadLock();

            ExpiringDictionary.SlideExpirationDate(key, true);

            return result;
        }

        public bool TryGetValue(TK key, out TV value)
        {
            bool result = default(bool);
            value = default(TV);

            AddRemoveLock.EnterUpgradeableReadLock();
            TV temp = default(TV);
            if (CachedItemList.ContainsKey(key))
            {
                result = true;
                temp = CachedItemList[key];
            }

            AddRemoveLock.ExitUpgradeableReadLock();

            ExpiringDictionary.SlideExpirationDate(key);

            value = temp;
            return result;
        }

        public void Set(TK key, TV value)
        {
            AddRemoveLock.EnterWriteLock();
            CachedItemList[key] = value;
            AddRemoveLock.ExitWriteLock();

            ExpiringDictionary.SlideExpirationDate(key);
        }

        public bool ContainsKey(TK key)
        {
            throw new NotImplementedException();
        }

        public void Add(TK key, TV value)
        {
            Set(key, value);
        }

        public void AddRange(Dictionary<TK, TV> values)
        {
            foreach (KeyValuePair<TK, TV> item in values)
            {
                Add(item.Key, item.Value);
            }
        }

        bool IDictionary<TK, TV>.Remove(TK key)
        {
            Remove(key);
            return true;
        }

        public bool Contains(TK key)
        {
            AddRemoveLock.EnterReadLock();
            bool result = CachedItemList.ContainsKey(key);

            if (result)
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

            AddRemoveLock.ExitReadLock();

            return result;
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }


        public void AddOrUpdate(TK key, TV value)
        {
            Set(key, value);
        }

        public bool AddIfNotExists(TK key, TV value)
        {
            bool result = default(bool);

            AddRemoveLock.EnterUpgradeableReadLock();

            if (!CachedItemList.ContainsKey(key))
            {
                Set(key, value);
                result = true;
            }

            AddRemoveLock.ExitUpgradeableReadLock();

            return result;
        }

        public TV GetOrAdd(TK key, TV value)
        {
            TV result;

            AddRemoveLock.EnterUpgradeableReadLock();

            if (!CachedItemList.ContainsKey(key))
            {
                Set(key, value);
            }

            result = CachedItemList[key];

            AddRemoveLock.ExitUpgradeableReadLock();

            ExpiringDictionary.SlideExpirationDate(key, true);

            return result;
        }

        public void Clear()
        {
            AddRemoveLock.EnterWriteLock();
            CachedItemList.Clear();
            AddRemoveLock.ExitWriteLock();
            ExpiringDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }

        public void Remove(TK key)
        {
            AddRemoveLock.EnterWriteLock();
            CachedItemList.Remove(key);
            AddRemoveLock.ExitWriteLock();
            ExpiringDictionary.Remove(key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Properties

        public int Count
        {
            get
            {
                AddRemoveLock.EnterReadLock();
                int result = CachedItemList.Count;
                AddRemoveLock.ExitReadLock();
                return result;
            }
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

        #region Implementation of IEnumerable

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}