using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UltraMapper.Json.Benchmarks
{
    //- STRING TO DATETIME CONVERSION PROBLEMS

    class Program
    {
        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public string CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

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

        public const int iterations = 1000 * 1000 * 10;

        static void Main( string[] args )
        {
            string json0 = @"{
                  ""Email"": ""mauro.sampietro@gmail.com"",
                  ""Active"": ""true"",
                  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
                  ""Roles"": 
                    [
                        ""User"",
                        ""Admin""
                    ]
                }";

            string json1 = @"
    		{
				""id"": ""0003"",
				""ppu"": 0.55,
					
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
					{ ""id"": ""5002"", ""type"": ""Glazed"" },
				]
			}
			";

            //Utf8JsonLibrary( json0 );
            UltraMapper<Account>( json0 );
            //Newtonsoft( json0 );
            //NetJson( json0 );

            //Utf8JsonLibrary( json1 );
            //UltraMapper<Item>( json1 );
            //Newtonsoft( json1 );
            //NetJson( json1 );

            Console.ReadLine();
        }

        private static void UltraMapper<T>( string json ) where T : class, new()
        {
            var sw = new Stopwatch();
            sw.Start();
            var jsonParser = new JsonSerializer();

            for( int i = 0; i < iterations; i++ )
            {
                jsonParser.Deserialize<T>( json );
            }

            sw.Stop();
            Console.WriteLine( $"UltraMapper: {sw.ElapsedMilliseconds}" );
        }

        private static void Newtonsoft( string json )
        {
            var sw = new Stopwatch();
            sw.Start();
            for( int i = 0; i < iterations; i++ )
                JsonConvert.DeserializeObject<Item>( json );
            sw.Stop();
            Console.WriteLine( $"NewtonSoft: {sw.ElapsedMilliseconds}" );
        }

        private static void Utf8JsonLibrary( string json )
        {
            var sw = new Stopwatch();
            sw.Start();

            for( int i = 0; i < iterations; i++ )
                Utf8Json.JsonSerializer.Deserialize<Item>( json );

            sw.Stop();
            Console.WriteLine( $".NET: {sw.ElapsedMilliseconds}" );
        }

        private static void NetJson( string json )
        {
            var sw = new Stopwatch();
            sw.Start();

            for( int i = 0; i < iterations; i++ )
                System.Text.Json.JsonSerializer.Deserialize<Item>( json );

            sw.Stop();
            Console.WriteLine( $".NET: {sw.ElapsedMilliseconds}" );
        }
    }
}
