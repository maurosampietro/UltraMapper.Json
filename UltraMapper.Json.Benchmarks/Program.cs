using BenchmarkDotNet.Running;
using System;

namespace UltraMapper.Json.Benchmarks
{
    class Program
    {
        static void Main( string[] args )
        {
            var summary = BenchmarkRunner.Run( typeof( Program ).Assembly );

            Console.ReadLine();
        }
    }
}