using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Newtonsoft.Json;
using System.Collections.Generic;
using UltraMapper.Parsing;

namespace UltraMapper.Json.Benchmarks
{
    //[SimpleJob( RuntimeMoniker.Net472, baseline: true )]
    //[SimpleJob( RuntimeMoniker.Net48 )]
    [SimpleJob( RuntimeMoniker.Net70 )]
    //[SimpleJob( RuntimeMoniker.Net80 )]
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
			""ppu"": ""55"",
					
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

        private static JsonSerializer<Item> jsonParser;

        [GlobalSetup]
        public void Setup()
        {
            jsonParser = new JsonSerializer<Item>(); 
        }


        [Benchmark]
        public void UltraMapper() => jsonParser.Deserialize( json );

        //[Benchmark]
        //public void ManualMapping()
        //{
        //    var Parser = new JsonParser();
        //    var parsedContent = (ComplexParam)Parser.Parse( json );

        //    Item result = new Item();

        //    foreach(var item in parsedContent.SubParams)
        //    {
        //        switch(item.Name)
        //        {
        //            case "ppu": result.ppu = ((SimpleParam)item).Value; break;
        //            case "id": result.id = ((SimpleParam)item).Value; break;
        //            case "batters":
        //            {
        //                result.batters = new Batters();

        //                foreach(var battersItems in ((ComplexParam)item).SubParams)
        //                {
        //                    result.batters.batter = new List<Ingredient>();

        //                    foreach(ComplexParam bat in ((ArrayParam)battersItems).Complex)
        //                    {
        //                        var newBatter = new Ingredient();

        //                        foreach(var subBat in bat.SubParams)
        //                        {
        //                            switch(subBat.Name)
        //                            {
        //                                case "id": newBatter.id = ((SimpleParam)subBat).Value; break;
        //                                case "type": newBatter.type = ((SimpleParam)subBat).Value; break;
        //                            }
        //                        }

        //                        result.batters.batter.Add( newBatter );
        //                    }
        //                }

        //                break;
        //            }
        //        }
        //    }
        //}

        //[Benchmark]
        //public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Deserialize<Item>( json );

        ////[Benchmark]
        //public void Newtonsoft() => JsonConvert.DeserializeObject<Item>( json );

        //[Benchmark]
        //public void NetJson() => System.Text.Json.JsonSerializer.Deserialize<Item>( json );
    }
}
