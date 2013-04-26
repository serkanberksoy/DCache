#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DCache;
using NewDCache;
using NewDCache.Interface;
using PartitionedMemoryCacher;

#endregion

namespace CacherPerformance
{
    internal class ParallelAddDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            Dictionary<int, string> d = new Dictionary<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.Add(i, "abc"));

            __sw.Stop();

            Console.WriteLine("Parallel Add Dictionary: {0}", __sw.ElapsedTicks);
        }
    }
    
    internal class ParallelAddMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            MemoryCacher<int, string> d = new MemoryCacher<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.Add(i, "abc"));

            __sw.Stop();

            Console.WriteLine("Parallel Add MemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }

    internal class ParallelAddPartitionedMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            PartitionedMemoryCacher<string> d = new PartitionedMemoryCacher<string>(CONCURRENCY_LEVEL, ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.Add(i, "abc"));

            __sw.Stop();

            Console.WriteLine("Parallel Add PartitionedMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }


    internal class ParallelAddConcurrentDictionary : PerformanceTestRunner
    {
        public override void Run()
        {
            ConcurrentDictionary<int, string> d = new ConcurrentDictionary<int, string>(CONCURRENCY_LEVEL, ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.TryAdd(i, "abc"));
            // Parallel.For(0, 1000000, i => d.GetOrAdd(i, "abc"));
            // Parallel.For(0, 1000000, i => d.AddOrUpdate(i, "abc", (key, oldValue) => "abc"));


            __sw.Stop();

            Console.WriteLine("Parallel Add ConcurrentDictionary: {0}", __sw.ElapsedTicks);
        }
    }

    internal class ParallelAddNewConcurrentMemoryCacher : PerformanceTestRunner
    {
        public override void Run()
        {
            IMemoryCacher<int, string> d = DCacheFactory.ConcurrentMemoryCacher.GetBasic<int, string>(ARRAY_SIZE);
            __sw.Reset();
            __sw.Start();

            Parallel.For(0, 1000000, i => d.Add(i, "abc"));

            __sw.Stop();

            Console.WriteLine("Parallel Add New ConcurrentMemoryCacher: {0}", __sw.ElapsedTicks);
        }
    }
}