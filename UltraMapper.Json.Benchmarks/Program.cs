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
            var summary = BenchmarkRunner.Run( typeof( Program ).Assembly, new DebugInProcessConfig() );

   //         string json0 = @"{
   //               ""Email"": ""james@example.com"",
   //               ""Active"": true,
   //               ""CreatedDate"": ""2013-01-20T00:00:00Z"",
   //               ""Roles"": [
   //                 ""User"",
   //                 ""Admin""
   //               ]
   //             }";

   //         string json1 = @"
			//{
			//	""id"": ""0003"",
			//	""ppu"": ""0.55"",
					
			//	""batters"":
			//	{
			//		""batter"":
			//		[
			//			{ ""id"": ""1001"", ""type"": ""Regular"" },
			//			{ ""id"": ""1002"", ""type"": ""Chocolate"" }
			//		]
			//	},
					
			//	""toppings"":
			//	[
			//		{ ""id"": ""5001"", ""type"": ""None"" },
			//		{ ""id"": ""5002"", ""type"": ""Glazed"" }
			//	]
			//}
			//";

   //         //Utf8JsonLibrary<Account>( json0 );
   //         //UltraMapper<Account>( json0 );
   //         //Newtonsoft<Account>( json0 );
   //         //NetJson<Account>( json0 );

   //         ////Utf8JsonLibrary( json1 );
   //         //NetJson<Item>( json1 );
   //         //UltraMapper<Item>( json1 );
   //         //Newtonsoft<Item>( json1 );


   //         //Console.ReadLine();
        }

        private static void UltraMapper<T>( string json ) where T : class, new()
        {
            var sw = new Stopwatch();
            sw.Start();
            var jsonParser = new JsonSerializer();

            T desItem = null;
            for( int i = 0; i < iterations; i++ )
                desItem = jsonParser.Deserialize<T>( json );

            sw.Stop();
            Console.WriteLine( $"UltraMapper: {sw.ElapsedMilliseconds}" );
        }

        private static void Newtonsoft<T>( string json ) where T : class, new()
        {
            var sw = new Stopwatch();
            sw.Start();

            T desItem = null;
            for( int i = 0; i < iterations; i++ )
                desItem = JsonConvert.DeserializeObject<T>( json );

            sw.Stop();
            Console.WriteLine( $"NewtonSoft: {sw.ElapsedMilliseconds}" );
        }

        private static void Utf8JsonLibrary<T>( string json ) where T : class, new()
        {
            var sw = new Stopwatch();
            sw.Start();


            T desItem = null;
            for( int i = 0; i < iterations; i++ )
                desItem = Utf8Json.JsonSerializer.Deserialize<T>( json );

            sw.Stop();
            Console.WriteLine( $".NET: {sw.ElapsedMilliseconds}" );
        }

        private static void NetJson<T>( string json ) where T : class, new()
        {
            var sw = new Stopwatch();
            sw.Start();

            T desItem = null;
            for( int i = 0; i < iterations; i++ )
                desItem = System.Text.Json.JsonSerializer.Deserialize<T>( json );

            sw.Stop();
            Console.WriteLine( $".NET: {sw.ElapsedMilliseconds}" );
        }
    }
}