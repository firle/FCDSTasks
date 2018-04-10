using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCorePrime
{
    class Program
    {

        static long currentNumber = 0;
        static long maxNumber = (long)Math.Pow(10, 6);

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {
            int minWorker, minIOC;
            int numThreads = 8;
            // Get the current settings.
            ThreadPool.GetMinThreads(out minWorker, out minIOC);
            // Change the minimum number of worker threads to four, but
            // keep the old setting for minimum asynchronous I/O 
            // completion threads.
            if (true)//ThreadPool.SetMinThreads(numThreads, minIOC))
            {
                Console.WriteLine($"Set number of Threads to '{numThreads}");

                var tasks = new List<Task>();


                for (int i = 0; i < numThreads; i++)
                {
                    tasks.Add(Task.Run((Action)PrintPrime));
                }

                var start = DateTime.Now;
                await Task.WhenAll(tasks);
                var end = DateTime.Now;

                Console.WriteLine($"Time at {numThreads} Tasks is {(end-start).TotalMilliseconds} ms");

            }
            else
            {
                Console.WriteLine("Setting maximum Number of Threads failed");
            }
        }

        public static void PrintPrime()
        {
            long value = 0;
            while(currentNumber<maxNumber)
            {
                value = Interlocked.Increment(ref currentNumber);
                if (IsPrime(value))
                    Console.WriteLine(value);
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
