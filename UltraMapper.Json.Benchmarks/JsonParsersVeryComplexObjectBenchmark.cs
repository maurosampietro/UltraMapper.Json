//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Jobs;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using UltraMapper.Parsing;

//namespace UltraMapper.Json.Benchmarks
//{
//    //[SimpleJob( RuntimeMoniker.Net472, baseline: true )]
//    //[SimpleJob( RuntimeMoniker.Net48 )]
//    [SimpleJob( RuntimeMoniker.Net70 )]
//    //[SimpleJob( RuntimeMoniker.Net80 )]
//    public class JsonParsersVeryComplexObjectBenchmark
//    {


//        public class Root
//        {
//            public int simple_field1 { get; set; }
//            public float simple_field2 { get; set; }
//            public string simple_field3 { get; set; }
//            public int[] array_of_integers { get; set; }
//            public Object_Field1 object_field1 { get; set; }
//            public Object_With_Fields1 object_with_fields1 { get; set; }
//            public int simple_field4 { get; set; }
//            public float simple_field5 { get; set; }
//            public string simple_field6 { get; set; }
//            public Array_Of_Objects[] array_of_objects { get; set; }
//            public Field10 field10 { get; set; }
//            public int field11 { get; set; }
//            public float field12 { get; set; }
//            public string field13 { get; set; }
//            public string[] array_of_strings { get; set; }
//            public Nested_Object_Field nested_object_field { get; set; }
//            public int simple_field7 { get; set; }
//            public float simple_field8 { get; set; }
//            public string simple_field9 { get; set; }
//            public Nested_Array_Of_Objects[] nested_array_of_objects { get; set; }
//        }

//        public class Object_Field1
//        {
//            public int subfield1 { get; set; }
//            public string subfield2 { get; set; }
//            public int[] subfield3 { get; set; }
//        }

//        public class Object_With_Fields1
//        {
//            public int field1 { get; set; }
//            public float field2 { get; set; }
//            public string field3 { get; set; }
//            public float[] array_of_floats { get; set; }
//            public Nested_Object nested_object { get; set; }
//            public Nested_Array[] nested_array { get; set; }
//            public string field7 { get; set; }
//            public int field8 { get; set; }
//            public Field9 field9 { get; set; }
//        }

//        public class Nested_Object
//        {
//            public int subfield1 { get; set; }
//            public string subfield2 { get; set; }
//            public float[] subfield3 { get; set; }
//        }

//        public class Field9
//        {
//            public int[] nested_field1 { get; set; }
//            public Nested_Field2 nested_field2 { get; set; }
//        }

//        public class Nested_Field2
//        {
//            public string key1 { get; set; }
//            public int[] key2 { get; set; }
//        }

//        public class Nested_Array
//        {
//            public string inner1 { get; set; }
//            public int[] inner2 { get; set; }
//            public float inner3 { get; set; }
//            public Inner4 inner4 { get; set; }
//        }

//        public class Inner4
//        {
//            public string deep1 { get; set; }
//            public int[] deep2 { get; set; }
//        }

//        public class Field10
//        {
//            public string nested_field1 { get; set; }
//            public int[] nested_field2 { get; set; }
//            public Nested_Field3 nested_field3 { get; set; }
//        }

//        public class Nested_Field3
//        {
//            public string key1 { get; set; }
//            public int[] key2 { get; set; }
//        }

//        public class Nested_Object_Field
//        {
//            public int[] nested_field1 { get; set; }
//            public Nested_Field21 nested_field2 { get; set; }
//        }

//        public class Nested_Field21
//        {
//            public string key1 { get; set; }
//            public int[] key2 { get; set; }
//        }

//        public class Array_Of_Objects
//        {
//            public string item1 { get; set; }
//            public int item2 { get; set; }
//        }

//        public class Nested_Array_Of_Objects
//        {
//            public string item { get; set; }
//        }


//        static string json = @"{""simple_field1"":42,""simple_field2"":314,""simple_field3"":""Hello, World!"",""array_of_integers"":[1,2,3,4,5],""object_field1"":{""subfield1"":7,""subfield2"":""Nested string"",""subfield3"":[9,8]},""object_with_fields1"":{""field1"":123,""field2"":777,""field3"":""Simple String"",""array_of_floats"":[111,222,333],""nested_object"":{""subfield1"":77,""subfield2"":""Nested string"",""subfield3"":[99,88]},""nested_array"":[{""inner1"":""value1"",""inner2"":[1,2,3]},{""inner3"":456,""inner4"":{""deep1"":""abc"",""deep2"":[5,6]}}],""field7"":""Another field"",""field8"":555,""field9"":{""nested_field1"":[11,22,33],""nested_field2"":{""key1"":""value1"",""key2"":[44,55]}}},""simple_field4"":789,""simple_field5"":2718,""simple_field6"":""Goodbye, World!"",""array_of_objects"":[{""item1"":""value1"",""item2"":42},{""item1"":""value2"",""item2"":43},{""item1"":""value3"",""item2"":44}],""field10"":{""nested_field1"":""Value"",""nested_field2"":[10,20,30],""nested_field3"":{""key1"":""value1"",""key2"":[40,50]}},""field11"":987,""field12"":6626e-34,""field13"":""Final string"",""array_of_strings"":[""one"",""two"",""three"",""four"",""five""],""nested_object_field"":{""nested_field1"":[1,2,3],""nested_field2"":{""key1"":""value1"",""key2"":[4,5]}},""simple_field7"":999,""simple_field8"":1618,""simple_field9"":""Last field"",""nested_array_of_objects"":[{""item"":""value1""},{""item"":""value2""},{""item"":""value3""}]}";


//        private static JsonSerializer<Root> jsonParser;

//        [GlobalSetup]
//        public void Setup()
//        {
//            jsonParser = new JsonSerializer<Root>();
//        }


//        [Benchmark]
//        public void UltraMapper() => jsonParser.Deserialize( json );

//        [Benchmark]
//        public void Utf8JsonLibrary() => Utf8Json.JsonSerializer.Deserialize<Root>( json );
        
//        //[Benchmark]
//        //public void Newtonsoft() => JsonConvert.DeserializeObject<Root>( json );

//        //[Benchmark]
//        //public void NetJson() => System.Text.Json.JsonSerializer.Deserialize<Item>( json );

//        //[Benchmark]
//        //public void ManualMapping()
//        //{
//        //    var Parser = new JsonParser();
//        //    var parsedContent = (ComplexParam)Parser.Parse( json );

//        //    Item result = new Item();

//        //    foreach(var item in parsedContent.SubParams)
//        //    {
//        //        switch(item.Name)
//        //        {
//        //            case "ppu": result.ppu = ((SimpleParam)item).Value; break;
//        //            case "id": result.id = ((SimpleParam)item).Value; break;
//        //            case "batters":
//        //            {
//        //                result.batters = new Batters();

//        //                foreach(var battersItems in ((ComplexParam)item).SubParams)
//        //                {
//        //                    result.batters.batter = new List<Ingredient>();

//        //                    foreach(ComplexParam bat in ((ArrayParam)battersItems).Complex)
//        //                    {
//        //                        var newBatter = new Ingredient();

//        //                        foreach(var subBat in bat.SubParams)
//        //                        {
//        //                            switch(subBat.Name)
//        //                            {
//        //                                case "id": newBatter.id = ((SimpleParam)subBat).Value; break;
//        //                                case "type": newBatter.type = ((SimpleParam)subBat).Value; break;
//        //                            }
//        //                        }

//        //                        result.batters.batter.Add( newBatter );
//        //                    }
//        //                }

//        //                break;
//        //            }
//        //        }
//        //    }
//        //}
//    }
//}
