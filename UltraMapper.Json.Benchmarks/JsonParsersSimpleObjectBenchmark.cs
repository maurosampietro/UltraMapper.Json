using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UltraMapper.Json.Benchmarks
{
    //[SimpleJob( RuntimeMoniker.Net472, baseline: true )]
    [SimpleJob( RuntimeMoniker.Net50 )]
    //[SimpleJob( RuntimeMoniker.Net60 )]
    public class JsonParsersSimpleObjectReadBenchmark
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

        private static readonly JsonSerializer<Account> jsonParser = new JsonSerializer<Account>();

        [Benchmark]
        public void UltraMapper() => jsonParser.Deserialize( json );

        [Benchmark]
        public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Deserialize<Account>( json );

        //[Benchmark]
        //public void Newtonsoft() => JsonConvert.DeserializeObject<Account>( json );

        //[Benchmark]
        //public void NetJson() => System.Text.Json.JsonSerializer.Deserialize<Account>( json );
    }
}
