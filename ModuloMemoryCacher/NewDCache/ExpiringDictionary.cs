#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NewDCache.Interface;

#endregion

namespace NewDCache
{
    public class ExpiringDictionary<TK, TV> : IExpiringDictionary<TK, TV>
    {
        private const int MAX_CLEARINTERVAL = 86400000;
        private const int MAX_DATAEXPIREDURATION_DAYS = 15;
        private const int MIN_CLEARINTERVAL_MS = 100;
        private const int MIN_DATAEXPIREDURATION_SECONDS = 5;

        #region events

        #region Delegates

        public delegate void BeforeItemsExpiredEventHandler(object sender, BeforeItemsExpiredEventArgs e);

        public delegate void ItemsExpiredEventHandler(object sender, ItemsExpiredEventArgs e);

        #endregion

        public event ItemsExpiredEventHandler ItemsExpired;
        public event BeforeItemsExpiredEventHandler BeforeItemsExpired;

        public void OnBeforeItemsExpired(BeforeItemsExpiredEventArgs e)
        {
            BeforeItemsExpiredEventHandler handler = BeforeItemsExpired;
            if (handler != null) handler(this, e);
        }

        public void OnItemsExpired(ItemsExpiredEventArgs e)
        {
            ItemsExpiredEventHandler handler = ItemsExpired;
            if (handler != null) handler(this, e);
        }

        #region Nested type: BeforeItemsExpiredEventArgs

        public class BeforeItemsExpiredEventArgs : EventArgs
        {
            public BeforeItemsExpiredEventArgs(List<TK> expiredKeys)
            {
                ExpiredKeys = expiredKeys;
            }

            public List<TK> ExpiredKeys { get; set; }
        }

        #endregion

        #region Nested type: ItemsExpiredEventArgs

        public class ItemsExpiredEventArgs : EventArgs
        {
            public ItemsExpiredEventArgs(Dictionary<TK, TV> expiredItems)
            {
                ExpiredItems = expiredItems;
            }

            public Dictionary<TK, TV> ExpiredItems { get; set; }
        }

        #endregion

        #endregion

        private readonly Timer _cleanupTimer;
        protected ConcurrentDictionary<TK, DateTime> ExpirationDateList;
        private TimeSpan _dataExpireDuration;
        private TimeSpan _timerInterval;

        public ExpiringDictionary(int initialCacheSize, int concurrencyLevel, bool isSliding, TimeSpan timerInterval,
                                  TimeSpan dataExpireDuration, bool isCustomTimerMethod)
        {
            if (IsTimerEnabled)
            {
                ExpirationDateList = new ConcurrentDictionary<TK, DateTime>(concurrencyLevel, initialCacheSize);

                IsSliding = isSliding;
                IsCustomTimerMethod = isCustomTimerMethod;

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

                _cleanupTimer = new Timer {Interval = TimerInterval.TotalMilliseconds};
                _cleanupTimer.Elapsed += TmrClearElapsed;
                _cleanupTimer.Start();
            }
        }

        #region IExpiringDictionary<TK,TV> Members

        public void Remove(TK key)
        {
            DateTime outDateVal;
            ExpirationDateList.TryRemove(key, out outDateVal);
        }

        public bool GetKeyExpired(TK key)
        {
            if (ExpirationDateList.ContainsKey(key))
            {
                return ExpirationDateList[key] > DateTime.Now;
            }
            else
            {
                return false;
            }
        }

        public void SlideExpirationDate(TK key)
        {
            if (IsTimerEnabled)
            {
                ExpirationDateList[key] = DateTime.Now.Add(_dataExpireDuration);
            }
        }

        public void SlideExpirationDate(TK key, bool isCheckSliding)
        {
            if (IsTimerEnabled && ((isCheckSliding && IsSliding) || !isCheckSliding))
            {
                ExpirationDateList[key] = DateTime.Now.Add(_dataExpireDuration);
            }
        }

        public void Clear()
        {
            ExpirationDateList.Clear();
        }

        public bool IsTimerEnabled
        {
            get { return true; }
        }

        public bool IsCustomTimerMethod { get; set; }
        public bool IsSliding { get; set; }

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
                    throw new ApplicationException(string.Format("Interval must be between {0} and {1} milliseconds!", MIN_CLEARINTERVAL_MS,
                                                                 MAX_CLEARINTERVAL));
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
                    throw new ApplicationException(string.Format("DataExpireDuration must be between {0} Seconds and {1} Days!",
                                                                 MIN_DATAEXPIREDURATION_SECONDS, MAX_DATAEXPIREDURATION_DAYS));
                }
            }
        }

        #endregion

        private void TmrClearElapsed(object sender, ElapsedEventArgs e)
        {
            _cleanupTimer.Stop();

            List<TK> expiredKeys = (ExpirationDateList
                .AsParallel()
                .Where(item => item.Value < DateTime.Now)
                .Select(item => item.Key))
                .ToList();

            OnBeforeItemsExpired(new BeforeItemsExpiredEventArgs(expiredKeys));

            _cleanupTimer.Start();
        }
    }
}