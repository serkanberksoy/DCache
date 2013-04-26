#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DCache;
using PartitionedMemoryCacher;

#endregion

namespace CacherPerformance
{
    internal class LoopUpdateDictionary : PerformanceTestRunner
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


            for (int i = 0; i < 1000000; i++)
            {
                d[i] = s;
            }

            __sw.Stop();

            Console.WriteLine("Loop Update Dictionary: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopUpdateMemoryCacher : PerformanceTestRunner
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

            for (int i = 0; i < 1000000; i++)
            {
                d[i] = s;
            }

            __sw.Stop();

            Console.WriteLine("Loop Update MemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }

    internal class LoopUpdatePartitionedMemoryCacher : PerformanceTestRunner
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

            for (int i = 0; i < 1000000; i++)
            {
                d.Set(i, s);
            }

            __sw.Stop();

            Console.WriteLine("Loop Update PartitionedMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopUpdateConcurrentDictionary : PerformanceTestRunner
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

            for (int i = 0; i < 1000000; i++)
            {
                d.TryUpdate(i, s, "a");
            }

            __sw.Stop();

            Console.WriteLine("Loop Update ConcurrentDictionary: {0}", __sw.ElapsedTicks);
        }
    }
}