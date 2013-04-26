#region

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace CacherPerformance
{
    public class KeyExistsControlTests : PerformanceTestRunner
    {
        public override void Run()
        {
            const int _max = 100000000;

            Dictionary<string, bool> dict = new Dictionary<string, bool>(StringComparer.Ordinal);
            SetTrue1(dict, "test");
            SetTrue2(dict, "test");

            var s1 = Stopwatch.StartNew();
            for (int i = 0; i < _max; i++)
            {
                SetTrue1(dict, "test");
            }
            s1.Stop();
            var s2 = Stopwatch.StartNew();
            for (int i = 0; i < _max; i++)
            {
                SetTrue2(dict, "test");
            }
            s2.Stop();

            Console.WriteLine("{0} ticks", ((double) s1.Elapsed.Ticks));
            Console.WriteLine("{0} ticks", ((double) s2.Elapsed.Ticks));
        }

        private static void SetTrue1(Dictionary<string, bool> dict, string key)
        {
            dict[key] = true;
        }

        private static void SetTrue2(Dictionary<string, bool> dict, string key)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = true;
            }
        }
    }
}