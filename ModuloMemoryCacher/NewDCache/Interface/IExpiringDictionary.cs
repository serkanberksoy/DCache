#region

using System;

#endregion

namespace NewDCache.Interface
{
    public interface IExpiringDictionary<TK, TV>
    {
        bool IsTimerEnabled { get; }
        bool IsCustomTimerMethod { get; set; }
        bool IsSliding { get; set; }
        TimeSpan TimerInterval { get; set; }
        TimeSpan DataExpireDuration { get; set; }
        event ExpiringDictionary<TK, TV>.ItemsExpiredEventHandler ItemsExpired;
        event ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventHandler BeforeItemsExpired;
        void OnBeforeItemsExpired(ExpiringDictionary<TK, TV>.BeforeItemsExpiredEventArgs e);
        void OnItemsExpired(ExpiringDictionary<TK, TV>.ItemsExpiredEventArgs e);

        void Remove(TK key);
        bool GetKeyExpired(TK key);

        void SlideExpirationDate(TK key);
        void SlideExpirationDate(TK key, bool isCheckSliding);

        void Clear();
    }
}