using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UltraMapper.Json.Benchmarks
{
    //- STRING TO DATETIME CONVERSION PROBLEMS

    class Program
    {
        static void Main( string[] args )
        {
            var summary = BenchmarkRunner.Run( typeof( Program ).Assembly, new DebugInProcessConfig() );
        }
    }
}