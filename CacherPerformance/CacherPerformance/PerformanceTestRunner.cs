#region

using System.Diagnostics;

#endregion

namespace CacherPerformance
{
    public abstract class PerformanceTestRunner
    {
        protected const int CONCURRENCY_LEVEL = 64;
        protected const int ARRAY_SIZE = 20000000;
        protected static readonly Stopwatch __sw = new Stopwatch();
        public abstract void Run();
    }
}