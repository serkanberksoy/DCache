#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DCache;
using NewDCache;
using NewDCache.Interface;
using PartitionedMemoryCacher;

#endregion

namespace CacherPerformance
{
    internal class LoopAddDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            Dictionary<int, string> d = new Dictionary<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            __sw.Stop();

            Console.WriteLine("Loop Add Dictionary: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopAddMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            MemoryCacher<int, string> d = new MemoryCacher<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            __sw.Stop();

            Console.WriteLine("Loop Add MemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }

    internal class LoopAddPartitionedMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            PartitionedMemoryCacher<string> d = new PartitionedMemoryCacher<string>(CONCURRENCY_LEVEL, ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            __sw.Stop();

            Console.WriteLine("Loop Add PartitionedMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }


    internal class LoopAddConcurrentDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            ConcurrentDictionary<int, string> d = new ConcurrentDictionary<int, string>(CONCURRENCY_LEVEL, ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                // d.GetOrAdd(i, "abc");
                d.TryAdd(i, "abc");
                // d.AddOrUpdate(i, "abc", (key, oldValue) => "abc");
            }

            __sw.Stop();

            Console.WriteLine("Loop Add ConcurrentDictionary: {0}", __sw.ElapsedTicks);
        }
    }

    internal class LoopAddNewConcurrentMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            IMemoryCacher<int, string> d = DCacheFactory.ConcurrentMemoryCacher.GetBasic<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            for (int i = 0; i < 1000000; i++)
            {
                d.Add(i, "abc");
            }

            __sw.Stop();

            Console.WriteLine("Loop Add New ConcurrentMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }
}