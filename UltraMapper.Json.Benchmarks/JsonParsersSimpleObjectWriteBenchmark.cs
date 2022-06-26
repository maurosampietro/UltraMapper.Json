using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UltraMapper.Json.Benchmarks
{
    //[SimpleJob( RuntimeMoniker.Net472, baseline: true )]
    [SimpleJob( RuntimeMoniker.Net50 )]
    //[SimpleJob( RuntimeMoniker.Net60 )]
    public class JsonParsersSimpleObjectWriteBenchmark
    {
        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public string CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        private Account account = new Account()
        {
            Email = "james@example.com",
            Active = true,
            CreatedDate = DateTime.Now.ToLongDateString(),
            Roles = new[] { "User", "Admin" }
        };

        private static readonly JsonSerializer<Account> jsonParser = new JsonSerializer<Account>();

        [Benchmark]
        public void UltraMapper() => jsonParser.Serialize( account );

        [Benchmark]
        public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Serialize( account );

        //[Benchmark]
        //public void Newtonsoft() => JsonConvert.SerializeObject( account );

        //[Benchmark]
        //public void NetJson() => System.Text.Json.JsonSerializer.Serialize( account );
    }
}
