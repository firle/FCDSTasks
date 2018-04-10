using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCorePrime
{
    class Program
    {
        // defining counting variables and iterations
        static long currentNumber = 0;
        static long primesFound = 0;
        static long maxNumber = (long)Math.Pow(10, 7);

        static void Main(string[] args)
        {
            //Settings-string
            //comma-seperated list of number of used Cores and at the end if multiThreading should be used
            var argString = "1,2,3,4,5,6,7,8,true";
            MainAsync(argString.Split(',')).GetAwaiter().GetResult();

            Console.ReadLine();
        }
        static async Task MainAsync(string[] args)
        {
            int maxCores, minIOC;

            //parse settings
            bool multiThreading = false;
            var threadList = new List<int>();

            if (args.Length < 2)
                return;

            for (int i = 0; i < args.Length-1; i++)
            {
                threadList.Add(int.Parse(args[i]));
            }
            multiThreading = Boolean.Parse(args[args.Length - 1]);
            //print settings
            Console.WriteLine($"multiThreading: {(multiThreading ? " On" : "Off")}");
            var s = string.Empty;
            threadList.ForEach(i => s += $"{i,3},");
            Console.WriteLine($"Test on {s} Threads");

            // Get the current ThreadPool-settings.
            ThreadPool.GetMinThreads(out maxCores, out minIOC);
            
            //run for each given setting
            foreach (var threads in threadList)
            {
                //run GC to avoid side effects from previous run
                GC.Collect();

                //reset counting variables
                currentNumber = 0;
                primesFound = 0;
                
                //set CPU-affinity
                uint affinity = 0;
                if(multiThreading)
                    affinity = (uint)Math.Pow(2, threads) - 1;
                else
                    affinity =((uint)Math.Pow(2, threads*2) - 1) & 0x5555;

                var cores = CountBits(affinity);
                var highestCore = Math.Floor(Math.Log(affinity, 2)) + 1;

                if (highestCore > maxCores)
                {
                    Console.WriteLine($"This System has not {cores} physical Cores!");
                    return;
                }

                Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)affinity;

                //set ThreadPool setting
                if (ThreadPool.SetMinThreads(threads, minIOC))
                {
                    var tasks = new List<Task>();

                    //take time and
                    //initialize given amount of Tasks
                    var start = DateTime.Now;
                    for (int i = 0; i < threads; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(PrintPrime));
                    }
                    //run tasks
                    await Task.WhenAll(tasks);
                    var end = DateTime.Now;

                    //print result
                    Console.WriteLine($"Cores: {cores,2} Tasks: {threads,2} Time: {(end - start).TotalMilliseconds,12:########.00} ms");
                    //Console.WriteLine($"Primes Found: {primesFound}");

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

        public static int CountBits(uint value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }
    }
}
