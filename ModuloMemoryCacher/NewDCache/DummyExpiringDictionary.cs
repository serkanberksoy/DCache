#region

using System;
using NewDCache.Interface;

#endregion

namespace NewDCache
{
    public class DummyExpiringDictionary<TK, TV> : IExpiringDictionary<TK, TV>
    {
        #region IExpiringDictionary<TK,TV> Members

        public event ExpiringDictionary<TK, TV>.ItemsExpiredEventHandler ItemsExpired;
        public event ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventHandler BeforeItemsExpired;
        public bool IsTimerEnabled { get; private set; }
        public bool IsCustomTimerMethod { get; set; }
        public bool IsSliding { get; set; }
        public TimeSpan TimerInterval { get; set; }
        public TimeSpan DataExpireDuration { get; set; }

        public void OnBeforeItemsExpired(ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnItemsExpired(ExpiringDictionary<TK, TV>.ItemsExpiredEventArgs e)
        {
        }

        public void Remove(TK key)
        {
        }

        public bool GetKeyExpired(TK key)
        {
            return true;
        }

        public void SlideExpirationDate(TK key)
        {
        }

        public void SlideExpirationDate(TK key, bool isCheckSliding)
        {
        }

        public void Clear()
        {
        }

        #endregion
    }
}