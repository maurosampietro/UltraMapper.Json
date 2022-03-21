using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UltraMapper.Json.Benchmarks
{
    [SimpleJob( RuntimeMoniker.Net462, baseline: true )]
    [SimpleJob( RuntimeMoniker.Net472 )]
    [SimpleJob( RuntimeMoniker.Net48 )]
    [SimpleJob( RuntimeMoniker.Net50 )]
    [SimpleJob( RuntimeMoniker.Net60 )]
    public class JsonParsersComplexObjectBenchmark
    {
        public class Item
        {
            public string id { get; set; }
            public string ppu { get; set; }
            public Batters batters { get; set; }
            public Ingredient[] toppings { get; set; }
        }

        public class Batters
        {
            public List<Ingredient> batter { get; set; }
        }

        public class Ingredient
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        static string json = @"
			{
				""id"": ""0003"",
				""ppu"": ""0.55"",
					
				""batters"":
				{
					""batter"":
					[
						{ ""id"": ""1001"", ""type"": ""Regular"" },
						{ ""id"": ""1002"", ""type"": ""Chocolate"" }
					]
				},
					
				""toppings"":
				[
					{ ""id"": ""5001"", ""type"": ""None"" },
					{ ""id"": ""5002"", ""type"": ""Glazed"" }
				]
			}
			";

        private static readonly JsonSerializer jsonParser = new JsonSerializer();

        [GlobalSetup]
        public void Setup()
        {

        }

        [Benchmark]
        public void UltraMapper() => jsonParser.Deserialize<Item>( json );

        [Benchmark]
        public void Newtonsoft() => JsonConvert.DeserializeObject<Item>( json );

        [Benchmark]
        public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Deserialize<Item>( json );

        [Benchmark]
        public void NetJson() => System.Text.Json.JsonSerializer.Deserialize<Item>( json );
    }
}
