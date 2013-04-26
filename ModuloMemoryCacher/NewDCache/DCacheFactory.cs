#region

using System;
using NewDCache.Interface;

#endregion

namespace NewDCache
{
    public static class DCacheFactory
    {
        private const int INITIAL_CACHE_SIZE = 128;
        private const int CONCURRENCY_LEVEL = 20;

        #region Nested type: ConcurrentMemoryCacher

        public static class ConcurrentMemoryCacher
        {
            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>()
            {
                return GetBasic<TK, TV>(INITIAL_CACHE_SIZE);
            }

            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>(int initialCacheSize)
            {
                return Get<TK, TV>(initialCacheSize, false, false, TimeSpan.Zero, TimeSpan.Zero);
            }

            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>(bool isTimerEnabled, int? initialCacheSize = null)
            {
                if (initialCacheSize == null)
                {
                    return Get<TK, TV>(INITIAL_CACHE_SIZE, false, false, TimeSpan.Zero, TimeSpan.Zero, false);
                }
                else
                {
                    return Get<TK, TV>(initialCacheSize.Value, isTimerEnabled, false, TimeSpan.Zero, TimeSpan.Zero, false);
                }
            }

            public static IMemoryCacher<TK, TV> Get<TK, TV>(int initialCacheSize, bool isTimerEnabled, bool isSliding,
                                                            TimeSpan timerInterval, TimeSpan dataExpireDuration)
            {
                return Get<TK, TV>(initialCacheSize, isTimerEnabled, isSliding, timerInterval, dataExpireDuration, false);
            }

            public static IMemoryCacher<TK, TV> Get<TK, TV>(int initialCacheSize, bool isTimerEnabled, bool isSliding,
                                                            TimeSpan timerInterval, TimeSpan dataExpireDuration, bool isCustomTimerMethod)
            {
                IExpiringDictionary<TK, TV> expiringDictionary;

                if (isTimerEnabled)
                {
                    expiringDictionary = new ExpiringDictionary<TK, TV>(initialCacheSize, CONCURRENCY_LEVEL, isSliding, timerInterval,
                                                                        dataExpireDuration, isCustomTimerMethod);
                }
                else
                {
                    expiringDictionary = new DummyExpiringDictionary<TK, TV>();
                }
                return new ConcurrentMemoryCacher<TK, TV>(initialCacheSize, CONCURRENCY_LEVEL, expiringDictionary);
            }
        }

        #endregion

        #region Nested type: MemoryCacher

        public static class MemoryCacher
        {
            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>()
            {
                return GetBasic<TK, TV>(INITIAL_CACHE_SIZE);
            }

            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>(int initialCacheSize)
            {
                return Get<TK, TV>(initialCacheSize, false, false, TimeSpan.Zero, TimeSpan.Zero, false);
            }

            public static IMemoryCacher<TK, TV> GetBasic<TK, TV>(bool isTimerEnabled, int? initialCacheSize = null)
            {
                if (initialCacheSize == null)
                {
                    return Get<TK, TV>(INITIAL_CACHE_SIZE, false, false, TimeSpan.Zero, TimeSpan.Zero, false);
                }
                else
                {
                    return Get<TK, TV>(initialCacheSize.Value, isTimerEnabled, false, TimeSpan.Zero, TimeSpan.Zero, false);
                }
            }

            public static IMemoryCacher<TK, TV> Get<TK, TV>(int initialCacheSize, bool isTimerEnabled, bool isSliding,
                                                            TimeSpan timerInterval, TimeSpan dataExpireDuration)
            {
                return Get<TK, TV>(initialCacheSize, isTimerEnabled, isSliding, timerInterval, dataExpireDuration, false);
            }

            public static IMemoryCacher<TK, TV> Get<TK, TV>(int initialCacheSize, bool isTimerEnabled, bool isSliding,
                                                            TimeSpan timerInterval, TimeSpan dataExpireDuration, bool isCustomTimerMethod)
            {
                IExpiringDictionary<TK, TV> expiringDictionary;

                if (isTimerEnabled)
                {
                    expiringDictionary = new ExpiringDictionary<TK, TV>(initialCacheSize, CONCURRENCY_LEVEL, isSliding, timerInterval,
                                                                        dataExpireDuration, isCustomTimerMethod);
                }
                else
                {
                    expiringDictionary = new DummyExpiringDictionary<TK, TV>();
                }

                return new MemoryCacher<TK, TV>(initialCacheSize, expiringDictionary);
            }
        }

        #endregion
    }
}