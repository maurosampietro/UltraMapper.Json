using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace UltraMapper.Json.Benchmarks
{
    class Program
    {
        static void Main( string[] args )
        {
            var summary = BenchmarkRunner.Run<JsonParsersComplexObjectBenchmark>( new DebugInProcessConfig() );

            Console.ReadLine();
        }
    }
}