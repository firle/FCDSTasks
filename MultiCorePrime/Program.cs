using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCorePrime
{
    class Program
    {

        static long currentNumber = 0;
        static long primesFound = 0;
        static long maxNumber = (long)Math.Pow(10, 7);

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {
            int minWorker, minIOC;
            int numThreads = 8;

            var numList = new int[] { 4 };// 1, 2, 3, 4 };//{ 1, 2, 3, 4, 5, 6, 7, 8 };
            // Change the minimum number of worker threads to four, but
            // keep the old setting for minimum asynchronous I/O 
            // completion threads.

            foreach (var numThread in numList)
            {
                GC.Collect();
                currentNumber = 0;
                primesFound = 0;
                // Get the current settings.
                ThreadPool.GetMinThreads(out minWorker, out minIOC);

                Console.WriteLine($"{minWorker}, {minIOC}");

                if (ThreadPool.SetMinThreads(numThread, minIOC))
                {
                    var tasks = new List<Task>();

                    var start = DateTime.Now;
                    for (int i = 0; i < numThreads; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(PrintPrime));
                    }

                    await Task.WhenAll(tasks);
                    var end = DateTime.Now;

                    Console.WriteLine($"Time at {numThread} Tasks is {(end - start).TotalMilliseconds} ms");
                    Console.WriteLine($"Primes Found: {primesFound}");

                }
                else
                {
                    Console.WriteLine("Setting maximum Number of Threads failed");
                }
            }
        }

        public static void PrintPrime()
        {
            long value = 0;
            while(currentNumber<maxNumber)
            {
                value = Interlocked.Increment(ref currentNumber);
                if (IsPrime(value))
                    Interlocked.Increment(ref primesFound);
                    //Console.WriteLine(value);
            }
        }

        public static bool IsPrime(long number)
        {
            if (number == 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var boundary = (long)Math.Floor(Math.Sqrt(number));

            for (int i = 3; i <= boundary; i += 2)
            {
                if (number % i == 0) return false;
            }

            return true;
        }
    }
}
