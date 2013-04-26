#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using DCache;

#endregion

namespace PartitionedMemoryCacher
{
    public class PartitionedMemoryCacher<T>
    {
        #region Constants and Properties

        protected const int DEFAULT_RADIX = 1;
        protected const int INITIAL_CACHE_SIZE = 100;
        private const int MAX_CLEARINTERVAL = 86400000;
        private const int MAX_DATAEXPIREDURATION_DAYS = 15;
        private const int MIN_CLEARINTERVAL_MS = 100;
        private const int MIN_DATAEXPIREDURATION_SECONDS = 5;
        private readonly IDictionary<int, MemoryCacher<int, T>> _dcacheArray;
        private readonly bool _isTimerEnabled;
        private readonly int _radix;
        private Timer _cleanupTimer;
        private TimeSpan _dataExpireDuration;
        private TimeSpan _timerInterval;

        #endregion

        #region Constructors

        private PartitionedMemoryCacher()
        {
            _radix = DEFAULT_RADIX;
            _isTimerEnabled = true;
            Sliding = false;
            _timerInterval = new TimeSpan(0, 0, 0, 0, 100);
            DataExpireDuration = new TimeSpan(0, 0, 0, 5, 0);
        }

        public PartitionedMemoryCacher(int radix, bool isTimerEnabled)
            : this(radix, isTimerEnabled, false, TimeSpan.Zero, TimeSpan.Zero)
        {
        }

        public PartitionedMemoryCacher(int radix, int initialCacheSize)
            : this(radix, initialCacheSize, false, false, TimeSpan.Zero, TimeSpan.Zero)
        {
        }

        public PartitionedMemoryCacher(int radix, bool isTimerEnabled, bool isSliding, TimeSpan timerInterval,
                                       TimeSpan dataExpireDuration)
            : this(radix, INITIAL_CACHE_SIZE, isTimerEnabled, isSliding, timerInterval, dataExpireDuration)
        {
        }

        public PartitionedMemoryCacher(int radix, int initialCacheSize, bool isTimerEnabled, bool isSliding,
                                       TimeSpan timerInterval, TimeSpan dataExpireDuration)
            : this()
        {
            _radix = radix;
            Dictionary<int, MemoryCacher<int, T>> temp = new Dictionary<int, MemoryCacher<int, T>>();

            for (int i = 0; i < _radix; i++)
            {
                temp.Add(i, new MemoryCacher<int, T>(initialCacheSize/_radix, isTimerEnabled, isSliding,
                                                     timerInterval, dataExpireDuration));
            }

            _dcacheArray = new ReadOnlyDictionary<int, MemoryCacher<int, T>>(temp);

            _isTimerEnabled = isTimerEnabled;

            if (!IsTimerEnabled)
                return;

            Sliding = isSliding;
            if (timerInterval == TimeSpan.Zero)
            {
                timerInterval = new TimeSpan(0, 0, 0, 0, 100);
            }

            if (dataExpireDuration == TimeSpan.Zero)
            {
                dataExpireDuration = new TimeSpan(0, 0, 0, 5, 0);
            }

            DataExpireDuration = dataExpireDuration;
            _timerInterval = timerInterval;

            // TODO: init timer çalıştırmıyoruz çünkü bunun yerine yukarıda instance alırken full çağırıyoruz
            //this.InitTimer();
        }

        #endregion

        public bool IsTimerEnabled
        {
            get { return _isTimerEnabled; }
        }

        public bool Sliding { get; set; }

        public TimeSpan DataExpireDuration
        {
            get { return _dataExpireDuration; }
            set
            {
                if (value.TotalDays > 15.0 || value.TotalSeconds < 5.0)
                    throw new ApplicationException(
                        string.Format("DataExpireDuration must be between {0} Seconds and {1} Days!", 5, 15));
                _dataExpireDuration = value;
            }
        }

        public MemoryCacher<int, T> this[int i]
        {
            get { return _dcacheArray[i]; }
        }


        public int Count
        {
            get
            {
                int result = default(int);

                for (int i = 0; i < _dcacheArray.Count; i++)
                {
                    result += _dcacheArray[i].Count;
                }

                return result;
            }
        }

        public TimeSpan TimerInterval
        {
            get { return _timerInterval; }
            set
            {
                if (value.TotalMilliseconds > 86400000.0 || value.TotalMilliseconds < 100.0)
                    throw new ApplicationException(string.Format("Interval must be between {0} and {1} milliseconds!",
                                                                 100, 86400000));
                _timerInterval = value;
                _cleanupTimer.Stop();
                _cleanupTimer.Interval = value.TotalMilliseconds;
                _cleanupTimer.Start();
            }
        }

        public int GetInternalArrayCount()
        {
            return _dcacheArray.Count;
        }


        public List<T> GetValues(List<int> keys)
        {
            return new List<T>(GetValues(keys.ToArray()));
        }

        public T[] GetValues(int[] keys)
        {
            int length = keys.Length;
            T[] result = new T[length];

            for (int i = 0; i < length; ++i)
            {
                result[i] = GetValue(keys[i]);
            }

            return result;
        }


        public Dictionary<int, T> GetPairs(List<int> keys)
        {
            Dictionary<int, T> result = new Dictionary<int, T>();
            for (int i = 0; i < keys.Count; ++i)
            {
                result[keys[i]] = GetValue(keys[i]);
            }

            return result;
        }

        public T GetValue(int key)
        {
            return _dcacheArray[key%_radix].GetValue(key);
        }

        public void Set(int key, T value)
        {
            _dcacheArray[key%_radix].Set(key, value);
        }

        public void Add(int key, T value)
        {
            _dcacheArray[key%_radix].Add(key, value);
        }

        public void AddRange(Dictionary<int, T> values)
        {
            foreach (KeyValuePair<int, T> kvp in values)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        public void Remove(int key)
        {
            _dcacheArray[key%_radix].Remove(key);
        }

        public bool Contains(int key)
        {
            return _dcacheArray[key%_radix].Contains(key);
        }

        public void Clear()
        {
            for (int i = 0; i < _dcacheArray.Count; i++)
            {
                _dcacheArray[i].Clear();
            }
        }

        #region MemoryCacher

        /*
        

    private void InitTimer()
    {
      this._cleanupTimer = new System.Timers.Timer()
      {
        Interval = this.TimerInterval.TotalMilliseconds
      };
      this._cleanupTimer.Elapsed += new ElapsedEventHandler(this.TmrClearElapsed);
      this._cleanupTimer.Start();
    }

    private void TmrClearElapsed(object sender, ElapsedEventArgs e)
    {
      this._cleanupTimer.Stop();
      this.CleanupTimerElapsed();
      this._cleanupTimer.Start();
    }

    protected virtual void CleanupTimerElapsed()
    {
      this.AddRemoveLock.EnterUpgradeableReadLock();
      IEnumerable<TYpeKey> source = (IEnumerable<TYpeKey>) Enumerable.ToList<TYpeKey>(Enumerable.Select<KeyValuePair<TYpeKey, DateTime>, TYpeKey>(Enumerable.Where<KeyValuePair<TYpeKey, DateTime>>((IEnumerable<KeyValuePair<TYpeKey, DateTime>>) this.ExpirationDateList, (Func<KeyValuePair<TYpeKey, DateTime>, bool>) (item => item.Value < DateTime.Now)), (Func<KeyValuePair<TYpeKey, DateTime>, TYpeKey>) (item => item.Key)));
      if (Enumerable.Count<TYpeKey>(source) > 0)
      {
        this.AddRemoveLock.EnterWriteLock();
        foreach (TYpeKey key in source)
        {
          this.CachedItemList.Remove(key);
          this.ExpirationDateList.Remove(key);
        }
        this.AddRemoveLock.ExitWriteLock();
      }
      this.AddRemoveLock.ExitUpgradeableReadLock();
    }

        */

        #endregion
    }
}