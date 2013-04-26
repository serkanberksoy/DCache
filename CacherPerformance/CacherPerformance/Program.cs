#region

using System;

#endregion

namespace CacherPerformance
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("\n---------Loop Add Tests---------------");
            (new LoopAddDictionary()).Run();
            (new LoopAddConcurrentDictionary()).Run();
            (new LoopAddMemoryCacher()).Run();
            (new LoopAddPartitionedMemoryCacher()).Run();
            (new LoopAddNewConcurrentMemoryCacher()).Run();


            Console.WriteLine("\n---------Loop Get Tests---------------");
            (new LoopGetDictionary()).Run();
            (new LoopGetConcurrentDictionary()).Run();
            (new LoopGetMemoryCacher()).Run();
            (new LoopGetPartitionedMemoryCacher()).Run();

            Console.WriteLine("\n---------Loop Update Tests---------------");
            (new LoopUpdateDictionary()).Run();
            (new LoopUpdateConcurrentDictionary()).Run();
            (new LoopUpdateMemoryCacher()).Run();
            (new LoopUpdatePartitionedMemoryCacher()).Run();


            Console.WriteLine("\n\n---------Parallel Add Tests---------------");
            //(new ParallelAddDictionary()).Run();
            (new ParallelAddConcurrentDictionary()).Run();
            (new ParallelAddMemoryCacher()).Run();
            (new ParallelAddPartitionedMemoryCacher()).Run();
            (new ParallelAddNewConcurrentMemoryCacher()).Run();

            Console.WriteLine("\n---------Parallel Get Tests---------------");
            //(new ParallelGetDictionary()).Run();
            (new ParallelGetConcurrentDictionary()).Run();
            (new ParallelGetMemoryCacher()).Run();
            (new ParallelGetPartitionedMemoryCacher()).Run();


            Console.WriteLine("\n---------Parallel Update Tests---------------");
            //(new ParallelUpdateDictionary()).Run();
            (new ParallelUpdateConcurrentDictionary()).Run();
            (new ParallelUpdateMemoryCacher()).Run();
            (new ParallelUpdatePartitionedMemoryCacher()).Run();


            Console.WriteLine("\n\n---------Check Existing Key Tests--------------- DISABLED");
            //  (new KeyExistsControlTests()).Run();

            Console.ReadKey();
        }
    }
}