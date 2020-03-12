using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using System.Threading;

namespace Myriad.Benchmark
{
    public class Benchmarker
    {
        [Benchmark]
        public void Run()
        {
            Thread.Sleep(50);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmarker>();
        }
    }
}
