#region

using System;
using System.Collections.Generic;

#endregion

namespace NewDCache.Interface
{
    public interface IMemoryCacher<TK, TV> : IDictionary<TK, TV>
    {
        bool IsTimerEnabled { get; }
        bool IsSliding { get; set; }
        TimeSpan TimerInterval { get; set; }
        TimeSpan DataExpireDuration { get; set; }
        new int Count { get; }
        List<TV> GetValues(List<TK> keys);
        TV[] GetValues(TK[] keys);
        Dictionary<TK, TV> GetPairs(List<TK> keys);
        TV GetValue(TK key);
        void Set(TK key, TV value);
        void AddRange(Dictionary<TK, TV> values);
        bool Contains(TK key);

        new bool TryGetValue(TK key, out TV value);
        void AddOrUpdate(TK key, TV value);
        bool AddIfNotExists(TK key, TV value);
        TV GetOrAdd(TK key, TV value);
    }
}