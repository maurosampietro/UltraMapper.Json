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
using System.Linq;
using System.Reflection;
using System.Text;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.Parsing;

namespace UltraMapper.Json.Benchmarks
{
    //- STRING TO DATETIME CONVERSION PROBLEMS
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

    class Program
    {


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
            //GetMidStruct<Item>();

            var summary = BenchmarkRunner.Run<JsonParsersComplexObjectBenchmark>( new DebugInProcessConfig() );

            var jsonSer = new JsonSerializer();
            var item = jsonSer.Deserialize<Item>( json );
            var rijson = jsonSer.Serialize( item );

            Console.ReadLine();
        }


        private static Dictionary<Type, IParsedParam> _templates
            = new Dictionary<Type, IParsedParam>();

        private static void GetMidStruct<T>()
            => GetMidStruct( typeof( T ) );

        private static void GetMidStruct( Type type )
        {
            if( _templates.ContainsKey( type ) )
                return;

            var convention = new Mapper().Config.Conventions.OfType<DefaultConvention>().First();
            var relevant = convention.SourceMemberProvider.GetMembers( type ).ToList();
            //.Where(m=>m.GetCustomAttributes().Any(a=>!(a is ignore ) ));

            var cp = new ComplexParam() { SubParams = new List<IParsedParam>() };

            foreach( var item in relevant )
            {
                var itemType = item.GetMemberType();

                if( itemType.IsBuiltIn( true ) )
                {
                    cp.SubParams.Add( new SimpleParam() { Name = item.Name } );
                }
                else if( itemType.IsEnumerable() )
                {
                    cp.SubParams.Add( new ArrayParam() { Name = item.Name } );
                    GetMidStruct( item.GetMemberType().GetCollectionGenericType() );
                }
                else
                {
                    cp.SubParams.Add( new ComplexParam() { Name = item.Name } );
                    GetMidStruct( item.GetMemberType() );
                }

            }

            _templates.Add( type, cp );
        }


   
    }
}