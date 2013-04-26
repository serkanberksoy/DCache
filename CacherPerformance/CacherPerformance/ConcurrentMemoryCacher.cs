namespace DCache
{ /*
    public class ConcurrentMemoryCacher<TK, TV> : IDictionary<TK,TV>, IDictionary
    {
        #region Private Constants

        protected const int INITIAL_CACHE_SIZE = 100;
        private const int MAX_CLEARINTERVAL = 86400000;
        private const int MAX_DATAEXPIREDURATION_DAYS = 15;
        private const int MIN_CLEARINTERVAL_MS = 100;
        private const int MIN_DATAEXPIREDURATION_SECONDS = 5;
        private const int CONCURRENCY_LEVEL = 20;

        #endregion

        #region ItemsExpirationEvent
        public class ItemsExpiredEventArgs : EventArgs
        {
            public ItemsExpiredEventArgs(Dictionary<TK, TV> expiredItems)
            {
                ExpiredItems = expiredItems;
            }
            public Dictionary<TK, TV> ExpiredItems { get; set; }
        }

        public delegate void ItemsExpiredEventHandler(object sender, ItemsExpiredEventArgs e);
        public event ItemsExpiredEventHandler ItemsExpired;

        public void OnItemsExpired(ItemsExpiredEventArgs e)
        {
            ItemsExpiredEventHandler handler = ItemsExpired;
            if (handler != null) handler(this, e);
        }
        #endregion

        #region Private Properties

        private readonly bool _isTimerEnabled;
        private Timer _cleanupTimer;

        private TimeSpan _dataExpireDuration;
        private TimeSpan _timerInterval;

        protected ConcurrentDictionary<TK, TV> CachedItemList;
        protected ConcurrentDictionary<TK, DateTime> ExpirationDateList;


        #endregion

        #region Constructor

        private ConcurrentMemoryCacher()
        {
            _isTimerEnabled = true;
            Sliding = false;

            _timerInterval = new TimeSpan(0, 0, 0, 0, MIN_CLEARINTERVAL_MS);
            DataExpireDuration = new TimeSpan(0, 0, 0, MIN_DATAEXPIREDURATION_SECONDS, 0);
        }


        internal ConcurrentMemoryCacher(int initialCacheSize, bool isTimerEnabled, bool isSliding, TimeSpan timerInterval, TimeSpan dataExpireDuration)
            : this()
        {
            CachedItemList = new ConcurrentDictionary<TK, TV>(CONCURRENCY_LEVEL, initialCacheSize);
            ExpirationDateList = new ConcurrentDictionary<TK, DateTime>(CONCURRENCY_LEVEL, initialCacheSize);

            _isTimerEnabled = isTimerEnabled;

            if (IsTimerEnabled)
            {
                Sliding = isSliding;

                if (timerInterval == TimeSpan.Zero)
                {
                    timerInterval = new TimeSpan(0, 0, 0, 0, MIN_CLEARINTERVAL_MS);
                }

                if (dataExpireDuration == TimeSpan.Zero)
                {
                    dataExpireDuration = new TimeSpan(0, 0, 0, MIN_DATAEXPIREDURATION_SECONDS, 0);
                }

                DataExpireDuration = dataExpireDuration;
                _timerInterval = timerInterval;
                InitTimer();
            }
        }

        #endregion

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

            if (CachedItemList.ContainsKey(key))
            {
                result = CachedItemList[key];
            }

            if (Sliding)
            {
                SlideExpirationDate(key);
            }

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

            if (Sliding)
            {
                SlideExpirationDate(key);
            }

            value = temp;
            return result;
        }

        public void Set(TK key, TV value)
        {
            CachedItemList[key] = value;
            SlideExpirationDate(key);
        }

        public bool ContainsKey(TK key)
        {
            throw new NotImplementedException();
        }

        public void Add(TK key, TV value)
        {
            Set(key, value);
        }

        bool IDictionary<TK, TV>.Remove(TK key)
        {
            throw new NotImplementedException();
        }

        public void AddRange(Dictionary<TK, TV> values)
        {
            foreach (KeyValuePair<TK, TV> item in values)
            {
                Add(item.Key, item.Value);
            }
        }

        public void Remove(TK key)
        {
            TV outVal;
            DateTime outVal2;
            CachedItemList.TryRemove(key, out outVal);
            ExpirationDateList.TryRemove(key, out outVal2);
        }

        public bool Contains(TK key)
        {
            bool result = false;

            if (CachedItemList.ContainsKey(key))
            {
                if (IsTimerEnabled && ExpirationDateList.ContainsKey(key))
                {
                    result = ExpirationDateList[key] > DateTime.Now;
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object key)
        {
            throw new NotImplementedException();
        }

        public void Add(object key, object value)
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

            if (Sliding)
            {
                SlideExpirationDate(key);
            }

            return result;
        }

        public void Clear()
        {
            CachedItemList.Clear();
            ExpirationDateList.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        object IDictionary.this[object key]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
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

        public struct SMemoryCacheData
        {
            public TV CachedItem;
            public DateTime ExpireDate;
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        protected void SlideExpirationDate(TK key)
        {
            if (IsTimerEnabled)
            {
                ExpirationDateList[key] = DateTime.Now.Add(_dataExpireDuration);
            }
        }

        private void InitTimer()
        {
            _cleanupTimer = new Timer { Interval = TimerInterval.TotalMilliseconds };
            _cleanupTimer.Elapsed += TmrClearElapsed;
            _cleanupTimer.Start();
        }

        private void TmrClearElapsed(object sender, ElapsedEventArgs e)
        {
            _cleanupTimer.Stop();
            CleanupTimerElapsed();
            _cleanupTimer.Start();
        }

        protected virtual void CleanupTimerElapsed()
        {
            Dictionary<TK, TV> expiredItems = new Dictionary<TK, TV>();

            IEnumerable<TK> timedoutKeys = (ExpirationDateList
                                                .Where(item => item.Value < DateTime.Now)
                                                .Select(item => item.Key))
                                                .ToList();

            if (timedoutKeys.Any())
            {
                TV outVal;
                foreach (TK key in timedoutKeys)
                {
                    expiredItems.Add(key, CachedItemList[key]);
                    CachedItemList.TryRemove(key, out outVal);
                    ExpirationDateList.Remove(key);
                }
            }

            if (timedoutKeys.Any())
            {
                OnItemsExpired(new ItemsExpiredEventArgs(expiredItems));
            }

        }
        #endregion

        #region Public Properties

        public int Count
        {
            get
            {
                return CachedItemList.Count;
            }
        }

        public object SyncRoot { get; private set; }
        public bool IsSynchronized { get; private set; }

        ICollection IDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize { get; private set; }
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

        ICollection IDictionary.Keys
        {
            get { return (ICollection)CachedItemList.Keys; }
        }

        ICollection<TV> IDictionary<TK, TV>.Values
        {
            get { return CachedItemList.Values; }
        }

        public bool IsTimerEnabled
        {
            get { return _isTimerEnabled; }
        }

        public bool Sliding { get; set; }

        public TimeSpan TimerInterval
        {
            get { return _timerInterval; }
            set
            {
                if (value.TotalMilliseconds <= MAX_CLEARINTERVAL &&
                    value.TotalMilliseconds >= MIN_CLEARINTERVAL_MS)
                {
                    _timerInterval = value;
                    _cleanupTimer.Stop();
                    _cleanupTimer.Interval = value.TotalMilliseconds;
                    _cleanupTimer.Start();
                }
                else
                {
                    throw new ApplicationException(string.Format("Interval must be between {0} and {1} milliseconds!", MIN_CLEARINTERVAL_MS, MAX_CLEARINTERVAL));
                }
            }
        }

        public TimeSpan DataExpireDuration
        {
            get { return _dataExpireDuration; }
            set
            {
                if (value.TotalDays <= MAX_DATAEXPIREDURATION_DAYS &&
                    value.TotalSeconds >= MIN_DATAEXPIREDURATION_SECONDS)
                {
                    _dataExpireDuration = value;
                }
                else
                {
                    throw new ApplicationException(string.Format("DataExpireDuration must be between {0} Seconds and {1} Days!", MIN_DATAEXPIREDURATION_SECONDS, MAX_DATAEXPIREDURATION_DAYS));
                }
            }
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
    }*/
}