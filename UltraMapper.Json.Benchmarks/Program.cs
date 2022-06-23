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
using System.Reflection;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json.Benchmarks
{
    //- STRING TO DATETIME CONVERSION PROBLEMS

    class Program
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

        static void Main( string[] args )
        {
            var summary = BenchmarkRunner.Run<JsonParsersSimpleObjectWriteBenchmark>( new DebugInProcessConfig() );

            //var jsonSer = new JsonSerializer();
            //var item = jsonSer.Deserialize<Item>( json );
            //var rijson = jsonSer.Serialize( item );

            //Console.ReadLine();
        }

        private static void manualMapping()
        {

            var Parser = new JsonParser();
            var parsedContent = (ComplexParam)Parser.Parse( json );

            Item result = new Item();

            foreach( var item in parsedContent.SubParams )
            {
                switch( item.Name )
                {
                    case "ppu": result.ppu = ((SimpleParam)item).Value; break;
                    case "id": result.id = ((SimpleParam)item).Value; break;
                    case "batters":
                    {
                        result.batters = new Batters();

                        foreach( var battersItems in ((ComplexParam)item).SubParams )
                        {
                            result.batters.batter = new List<Ingredient>();

                            foreach( ComplexParam bat in ((ArrayParam)battersItems).Items )
                            {
                                var newBatter = new Ingredient();

                                foreach( var subBat in bat.SubParams )
                                {
                                    switch( subBat.Name )
                                    {
                                        case "id": newBatter.id = ((SimpleParam)subBat).Value; break;
                                        case "type": newBatter.type = ((SimpleParam)subBat).Value; break;
                                    }
                                }

                                result.batters.batter.Add( newBatter );
                            }
                        }

                        break;
                    }
                }
            }

        }
    }
}