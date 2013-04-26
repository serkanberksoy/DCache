#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DCache;
using PartitionedMemoryCacher;

#endregion

namespace CacherPerformance
{
    internal class ParallelUpdateDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            Dictionary<int, string> d = new Dictionary<int, string>(ARRAY_SIZE);

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            string s = string.Empty;

            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d[i] = s);

            __sw.Stop();

            Console.WriteLine("Parallel Update Dictionary: {0}", __sw.ElapsedTicks);
        }
    }


    internal class ParallelUpdateMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            MemoryCacher<int, string> d = new MemoryCacher<int, string>(ARRAY_SIZE);

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            string s = string.Empty;

            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d[i] = s);

            __sw.Stop();

            Console.WriteLine("Parallel Update MemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }

    internal class ParallelUpdatePartitionedMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            PartitionedMemoryCacher<string> d = new PartitionedMemoryCacher<string>(CONCURRENCY_LEVEL, ARRAY_SIZE);

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            string s = string.Empty;

            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.Set(i, s));

            __sw.Stop();

            Console.WriteLine("Parallel Update PartitionedMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }


    internal class ParallelUpdateConcurrentDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            ConcurrentDictionary<int, string> d = new ConcurrentDictionary<int, string>(CONCURRENCY_LEVEL, ARRAY_SIZE);

            for (int i = 0; i < 1000000; i++)
            {
                d.TryAdd(i, "abc");
            }

            string s = string.Empty;

            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.TryUpdate(i, s, "a"));

            __sw.Stop();

            Console.WriteLine("Parallel Update ConcurrentDictionary: {0}", __sw.ElapsedTicks);
        }
    }
}