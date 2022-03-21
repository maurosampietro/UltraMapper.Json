using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UltraMapper.Json.Benchmarks
{
    [SimpleJob( RuntimeMoniker.Net462, baseline: true )]
    [SimpleJob( RuntimeMoniker.Net472 )]
    [SimpleJob( RuntimeMoniker.Net48 )]
    [SimpleJob( RuntimeMoniker.Net50 )]
    [SimpleJob( RuntimeMoniker.Net60 )]
    [RPlotExporter]
    public class JsonParsersSimpleObjectBenchmark
    {
        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public string CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        static readonly string json = @"
        {
            ""Email"": ""james@example.com"",
            ""Active"": true,
            ""CreatedDate"": ""2013-01-20T00:00:00Z"",
            ""Roles"": [
            ""User"",
            ""Admin""
            ]
        }";

        private static readonly JsonSerializer jsonParser = new JsonSerializer();

        [Benchmark]
        public void UltraMapper() => jsonParser.Deserialize<Account>( json );

        [Benchmark]
        public void Newtonsoft() => JsonConvert.DeserializeObject<Account>( json );

        [Benchmark]
        public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Deserialize<Account>( json );

        [Benchmark]
        public void NetJson()
        {
            System.Text.Json.JsonSerializer.Deserialize<Account>( json );
        }
    }
}
