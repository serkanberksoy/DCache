#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DCache;
using PartitionedMemoryCacher;

#endregion

namespace CacherPerformance
{
    internal class LoopGetDictionary : PerformanceTestRunner
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
                s = d[i];
            }

            __sw.Stop();

            Console.WriteLine("Loop Get Dictionary: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopGetMemoryCacher : PerformanceTestRunner
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
                s = d[i];
            }

            __sw.Stop();

            Console.WriteLine("Loop Get MemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }

    internal class LoopGetPartitionedMemoryCacher : PerformanceTestRunner
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
                s = d.GetValue(i);
            }

            __sw.Stop();

            Console.WriteLine("Loop Get PartitionedMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopGetConcurrentDictionary : PerformanceTestRunner
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
                d.TryGetValue(i, out s);
            }

            __sw.Stop();

            Console.WriteLine("Loop Get ConcurrentDictionary: {0}", __sw.ElapsedTicks);
        }
    }
}