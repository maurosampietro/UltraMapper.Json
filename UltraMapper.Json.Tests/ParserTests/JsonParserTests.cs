using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UltraMapper.Json.Tests.ParserTests.JsonManglers;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Parameters2;

namespace UltraMapper.Json.Tests.ParserTests
{
    public class JsonParserTests
    {
        private readonly IJsonMangler[] _manglers;

        private StringComparison _paramNamecomparisonMode
            = StringComparison.InvariantCultureIgnoreCase;

        public JsonParserTests( params IJsonMangler[] manglers )
        {
            _manglers = manglers ?? new[] { new DoNothingMangler() };
        }

        private string Mangle( string json )
        {
            if(_manglers == null) return json;

            return _manglers.Aggregate( json,
                ( aggJson, mangler ) => mangler.Mangle( aggJson ) );
        }

        [TestMethod]
        public void Example1ArrayPrimitiveType()
        {
            string json = "[ 100 , 200, 300, 400, 500 ]";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Simple.Count() == 5 );
            Assert.IsTrue( result.Simple[ 0 ].Value == "100" );
            Assert.IsTrue( result.Simple[ 1 ].Value == "200" );
            Assert.IsTrue( result.Simple[ 2 ].Value == "300" );
            Assert.IsTrue( result.Simple[ 3 ].Value == "400" );
            Assert.IsTrue( result.Simple[ 4 ].Value == "500" );
        }

        [TestMethod]
        public void QuotationContainsSpecialChars()
        {
            string json = @"{ ""param"":""}{\\][\"",:""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var param = result.Simple[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
            Assert.IsTrue( param.Value == @"}{\]["",:" );
        }

        [TestMethod]
        public void QuotationContainsControlChars()
        {
            string json = @"{""param"":""\\\""\b\f\n\r\t""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var param = result.Simple[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
            Assert.IsTrue( param.Value == "\\\"\b\f\n\r\t" );
        }

        [TestMethod]
        public void QuotationContainsUnicodeChars()
        {
            string json = @"{""param"":""\u0030\u0031""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var param = result.Simple[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
            Assert.IsTrue( param.Value == "\u0030\u0031" );
        }

        [TestMethod]
        public void EmptyObject()
        {
            string json = @"{}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Name.Equals( String.Empty ) );
            Assert.IsTrue( result.Count == 0 );
        }

        [TestMethod]
        public void EmptySubObject()
        {
            string json = @"
			{ 
				""emptyObject"" : {}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 1 );
            Assert.IsTrue( result.Complex[ 0 ].Name.Equals( "emptyObject", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Complex[ 0 ].Count == 0 );
        }

        [TestMethod]
        public void EmptySubObject2()
        {
            string json = @"
			{ 
				""emptyObject"" : {,,}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 1 );
            Assert.IsTrue( result.Complex[ 0 ].Name.Equals( "emptyObject", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Complex[ 0 ].Count == 0 );
        }

        [TestMethod]
        public void EmptyArray()
        {
            string json = @"[]";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 0 );
        }

        [TestMethod]
        public void EmptySubArray()
        {
            string json = @"
			{ 
				""emptyArray"" : []
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var subArrayParam2 = result.Array[ 0 ];

            Assert.IsTrue( subArrayParam2.Name.Equals( "emptyArray", _paramNamecomparisonMode ) );
            Assert.IsTrue( subArrayParam2.Count == 0 );
        }

        [TestMethod]
        public void SubArrays()
        {
            string json = @"
			[
				[1,2], [3,4], [5,6]
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Array.Count == 3 );

            var item1 = result.Array[ 0 ];
            Assert.IsTrue( item1.Simple.Count == 2 );
            Assert.IsTrue( item1.Simple[ 0 ].Value == "1" );
            Assert.IsTrue( item1.Simple[ 1 ].Value == "2" );

            var item2 = result.Array[ 1 ];
            Assert.IsTrue( item2.Simple.Count == 2 );
            Assert.IsTrue( item2.Simple[ 0 ].Value == "3" );
            Assert.IsTrue( item2.Simple[ 1 ].Value == "4" );

            var item3 = result.Array[ 2 ];
            Assert.IsTrue( item3.Simple.Count == 2 );
            Assert.IsTrue( item3.Simple[ 0 ].Value == "5" );
            Assert.IsTrue( item3.Simple[ 1 ].Value == "6" );
        }

        [TestMethod]
        public void SubArraysQuotedUnquotedElements()
        {
            string json = @"
			[
				[""1"",""2""], [""3"",4], [5,""6""]
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Array.Count == 3 );

            var item1 = result.Array[ 0 ];
            Assert.IsTrue( item1.Simple.Count == 2 );
            Assert.IsTrue( item1.Simple[ 0 ].Value == "1" );
            Assert.IsTrue( item1.Simple[ 1 ].Value == "2" );

            var item2 = result.Array[ 1 ];
            Assert.IsTrue( item2.Simple.Count == 2 );
            Assert.IsTrue( item2.Simple[ 0 ].Value == "3" );
            Assert.IsTrue( item2.Simple[ 1 ].Value == "4" );

            var item3 = result.Array[ 2 ];
            Assert.IsTrue( item3.Simple.Count == 2 );
            Assert.IsTrue( item3.Simple[ 0 ].Value == "5" );
            Assert.IsTrue( item3.Simple[ 1 ].Value == "6" );
        }

        [TestMethod]
        public void MultiDimensionalArray()
        {
            var json = @"
            {
                ""name"" : ""blogger"",
                ""users"" : 
                [
                    [""admins"", ""1"", ""2"" , ""3""],
		            [""editors"", ""4"", ""5"" , ""6""],
	            ]
            }";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 2 );

            Assert.IsTrue( result.Simple[ 0 ].Name.Equals( "name", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 0 ].Value == "blogger" );

            var userArray = result.Array[ 0 ];
            Assert.IsTrue( userArray.Name.Equals( "users", _paramNamecomparisonMode ) );
            Assert.IsTrue( userArray.Count == 2 );

            var subArray1 = userArray.Array[ 0 ];
            Assert.IsTrue( subArray1.Simple.Count() == 4 );
            Assert.IsTrue( subArray1.Simple[ 0 ].Value == "admins" );
            Assert.IsTrue( subArray1.Simple[ 1 ].Value == "1" );
            Assert.IsTrue( subArray1.Simple[ 2 ].Value == "2" );
            Assert.IsTrue( subArray1.Simple[ 3 ].Value == "3" );

            var subArray2 = userArray.Array[ 1 ];
            Assert.IsTrue( subArray2.Count == 4 );
            Assert.IsTrue( subArray2.Simple[ 0 ].Value == "editors" );
            Assert.IsTrue( subArray2.Simple[ 1 ].Value == "4" );
            Assert.IsTrue( subArray2.Simple[ 2 ].Value == "5" );
            Assert.IsTrue( subArray2.Simple[ 3 ].Value == "6" );
        }

        [TestMethod]
        public void Example2ArrayOfComplexObjects()
        {
            string json = @"
			[
				{
					""color"": ""red"",
					""value"": ""#f00""
				},
				{
					""color"": ""green"",
					""value"": ""#0f0""
				},
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 2 );

            var item1 = result.Complex[ 0 ];
            Assert.IsTrue( item1.Simple[ 0 ].Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( item1.Simple[ 0 ].Value == "red" );

            Assert.IsTrue( item1.Simple[ 1 ].Name.Equals( "value", _paramNamecomparisonMode ) );
            Assert.IsTrue( item1.Simple[ 1 ].Value == "#f00" );

            var item2 = result.Complex[ 1 ];
            Assert.IsTrue( item2.Simple[ 0 ].Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( item2.Simple[ 0 ].Value == "green" );

            Assert.IsTrue( item2.Simple[ 1 ].Name.Equals( "value", _paramNamecomparisonMode ) );
            Assert.IsTrue( item2.Simple[ 1 ].Value == "#0f0" );
        }

        [TestMethod]
        public void Example3Object()
        {
            string json = @" 
			{
				unquotedParam: unquotedValue,
			    ""quotedParam"": unquotedValue,
				""quotedParam2"": ""quotedValue"",
				escapedComma: "","",
                escapedQuoteAndComma: ""\"",\""""
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Name.Equals( String.Empty, _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Count == 5 );

            var param1 = result.Simple[ 0 ];
            Assert.IsTrue( param1.Name.Equals( "unquotedParam", _paramNamecomparisonMode ) );
            Assert.IsTrue( param1.Value == "unquotedValue" );

            var param2 = result.Simple[ 1 ];
            Assert.IsTrue( param2.Name.Equals( "quotedParam", _paramNamecomparisonMode ) );
            Assert.IsTrue( param2.Value == "unquotedValue" );

            var param3 = result.Simple[ 2 ];
            Assert.IsTrue( param3.Name.Equals( "quotedParam2", _paramNamecomparisonMode ) );
            Assert.IsTrue( param3.Value == "quotedValue" );

            var param4 = result.Simple[ 3 ];
            Assert.IsTrue( param4.Name.Equals( "escapedComma", _paramNamecomparisonMode ) );
            Assert.IsTrue( param4.Value == "," );

            var param5 = result.Simple[ 4 ];
            Assert.IsTrue( param5.Name.Equals( "escapedQuoteAndComma", _paramNamecomparisonMode ) );
            Assert.IsTrue( param5.Value == "\",\"" );
        }

        [TestMethod]
        public void Example3ObjectWithSubobject()
        {
            string json = @" 
			{
				""color"": ""red"",
				""user"":
				{
					""name"":""Robert"",
					""age"":32
				}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var param1 = result.Simple[ 0 ];
            var param2 = result.Complex[ 0 ];

            Assert.IsTrue( param1.Name == "color" );
            Assert.IsTrue( param1.Value == "red" );
            Assert.IsTrue( param2.Simple[ 0 ].Name == "name" );
            Assert.IsTrue( param2.Simple[ 0 ].Value == "Robert" );
            Assert.IsTrue( param2.Simple[ 1 ].Name == "age" );
            Assert.IsTrue( param2.Simple[ 1 ].Value == "32" );
        }

        [TestMethod]
        public void Example4HighlyNestedComplexObject()
        {
            string json = @"
			{
				""id"": ""0001"",
				""ppu"": 55,
		
				""batters"":
				{
					""batter"":
					[
						{ ""id"": ""1001"", ""type"": ""Regular"" },
						{ ""id"": ""1002"", ""type"": ""Chocolate"" },
					]
				},
				
				""toppings"":
				[
					{ ""id"": ""5001"", ""type"": ""None"" },
					{ ""id"": ""5002"", ""type"": ""Glazed"" },
				]
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 4 );

            Assert.IsTrue( result.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 0 ].Value == "0001" );

            Assert.IsTrue( result.Simple[ 1 ].Name.Equals( "ppu", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 1 ].Value == "55" );

            var complexParam = result.Complex[ 0 ];
            Assert.IsTrue( complexParam.Count == 1 );
            Assert.IsTrue( complexParam.Name.Equals( "batters", _paramNamecomparisonMode ) );

            var subArray = complexParam.Array[ 0 ];
            Assert.IsTrue( subArray.Count == 2 );
            Assert.IsTrue( subArray.Name.Equals( "batter", _paramNamecomparisonMode ) );

            var complexSubArrayItem1 = subArray.Complex[ 0 ];
            Assert.IsTrue( complexSubArrayItem1.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simple[ 0 ].Value == "1001" );

            Assert.IsTrue( complexSubArrayItem1.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simple[ 1 ].Value == "Regular" );

            var complexSubArrayItem2 = subArray.Complex[ 1 ];
            Assert.IsTrue( complexSubArrayItem2.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simple[ 0 ].Value == "1002" );

            Assert.IsTrue( complexSubArrayItem2.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simple[ 1 ].Value == "Chocolate" );

            var array = result.Array[ 0 ];
            Assert.IsTrue( array.Count == 2 );
            Assert.IsTrue( array.Name.Equals( "toppings", _paramNamecomparisonMode ) );

            var complexArrayItem1 = array.Complex[ 0 ];
            Assert.IsTrue( complexArrayItem1.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simple[ 0 ].Value == "5001" );

            Assert.IsTrue( complexArrayItem1.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simple[ 1 ].Value == "None" );

            var complexArrayItem2 = array.Complex[ 1 ];
            Assert.IsTrue( complexArrayItem2.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simple[ 0 ].Value == "5002" );

            Assert.IsTrue( complexArrayItem2.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simple[ 1 ].Value == "Glazed" );
        }

        [TestMethod]
        public void Example5ArrayOfHighlyNestedComplexObjects()
        {
            string json = @"
			[
				{
					""id"": ""0003"",
					""ppu"": 55,
					
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
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Count == 1 );

            var complexParam = result.Complex[ 0 ];
            Assert.IsTrue( complexParam.Count == 4 );

            Assert.IsTrue( complexParam.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexParam.Simple[ 0 ].Value == "0003" );

            Assert.IsTrue( complexParam.Simple[ 1 ].Name.Equals( "ppu", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexParam.Simple[ 1 ].Value == "55" );

            var subComplexParam2 = complexParam.Complex[ 0 ];
            Assert.IsTrue( subComplexParam2.Name.Equals( "batters", _paramNamecomparisonMode ) );

            var subArray = subComplexParam2.Array[ 0 ];
            Assert.IsTrue( subArray.Complex.Count == 2 );
            Assert.IsTrue( subArray.Name.Equals( "batter", _paramNamecomparisonMode ) );

            var complexSubArrayItem1 = subArray.Complex[ 0 ];
            Assert.IsTrue( complexSubArrayItem1.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simple[ 0 ].Value == "1001" );

            Assert.IsTrue( complexSubArrayItem1.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simple[ 1 ].Value == "Regular" );

            var complexSubArrayItem2 = subArray.Complex[ 1 ];
            Assert.IsTrue( complexSubArrayItem2.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simple[ 0 ].Value == "1002" );

            Assert.IsTrue( complexSubArrayItem2.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simple[ 1 ].Value == "Chocolate" );

            var subArray2 = complexParam.Array[ 0 ];
            Assert.IsTrue( subArray2.Complex.Count == 2 );
            Assert.IsTrue( subArray2.Name.Equals( "toppings", _paramNamecomparisonMode ) );

            var complexArrayItem1 = subArray2.Complex[ 0 ];
            Assert.IsTrue( complexArrayItem1.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simple[ 0 ].Value == "5001" );

            Assert.IsTrue( complexArrayItem1.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simple[ 1 ].Value == "None" );

            var complexArrayItem2 = subArray2.Complex[ 1 ];
            Assert.IsTrue( complexArrayItem2.Simple[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simple[ 0 ].Value == "5002" );

            Assert.IsTrue( complexArrayItem2.Simple[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simple[ 1 ].Value == "Glazed" );
        }

        [TestMethod]
        public void SetBoolLiterally()
        {
            string json = @"
			{
				""isParam1Set"": true,
				""isParam2Set"": false,
	            ""quotedTrue"": ""true"",
				""quotedFalse"": ""false""
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Simple[ 0 ].Name.Equals( "isParam1Set", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 0 ].Value == "true" );

            Assert.IsTrue( result.Simple[ 1 ].Name.Equals( "isParam2Set", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 1 ].Value == "false" );

            Assert.IsTrue( result.Simple[ 2 ].Name.Equals( "quotedTrue", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 2 ].Value == "true" );

            Assert.IsTrue( result.Simple[ 3 ].Name.Equals( "quotedFalse", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 3 ].Value == "false" );
        }

        [TestMethod]
        public void SetParamToNull()
        {
            string json = @"
			{
				""color"": null,
				""value"": ""null""
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            Assert.IsTrue( result.Simple[ 0 ].Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 0 ].Value == null );

            Assert.IsTrue( result.Simple[ 1 ].Name.Equals( "value", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simple[ 1 ].Value == "null" );
        }

        [TestMethod]
        public void SetArrayItemToNull()
        {
            string json = @"
			{
				""colors"":
                [
                    ""red"",
                    ""green"",
                    null,
                    ""yellow""
				]
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

            var colorsArray = result.Array[ 0 ];

            Assert.IsTrue( colorsArray.Name.Equals( "colors", _paramNamecomparisonMode ) );
            Assert.IsTrue( colorsArray.Simple[ 0 ].Value == "red" );
            Assert.IsTrue( colorsArray.Simple[ 1 ].Value == "green" );
            Assert.IsTrue( colorsArray.Simple[ 2 ].Value == null );
            Assert.IsTrue( colorsArray.Simple[ 3 ].Value == "yellow" );
        }



        public class Root
        {
            public int simple_field1 { get; set; }
            public float simple_field2 { get; set; }
            public string simple_field3 { get; set; }
            public int[] array_of_integers { get; set; }
            public Object_Field1 object_field1 { get; set; }
            public Object_With_Fields1 object_with_fields1 { get; set; }
            public int simple_field4 { get; set; }
            public float simple_field5 { get; set; }
            public string simple_field6 { get; set; }
            public Array_Of_Objects[] array_of_objects { get; set; }
            public Field10 field10 { get; set; }
            public int field11 { get; set; }
            public float field12 { get; set; }
            public string field13 { get; set; }
            public string[] array_of_strings { get; set; }
            public Nested_Object_Field nested_object_field { get; set; }
            public int simple_field7 { get; set; }
            public float simple_field8 { get; set; }
            public string simple_field9 { get; set; }
            public Nested_Array_Of_Objects[] nested_array_of_objects { get; set; }
        }

        public class Object_Field1
        {
            public int subfield1 { get; set; }
            public string subfield2 { get; set; }
            public int[] subfield3 { get; set; }
        }

        public class Object_With_Fields1
        {
            public int field1 { get; set; }
            public float field2 { get; set; }
            public string field3 { get; set; }
            public float[] array_of_floats { get; set; }
            public Nested_Object nested_object { get; set; }
            public Nested_Array[] nested_array { get; set; }
            public string field7 { get; set; }
            public int field8 { get; set; }
            public Field9 field9 { get; set; }
        }

        public class Nested_Object
        {
            public int subfield1 { get; set; }
            public string subfield2 { get; set; }
            public float[] subfield3 { get; set; }
        }

        public class Field9
        {
            public int[] nested_field1 { get; set; }
            public Nested_Field2 nested_field2 { get; set; }
        }

        public class Nested_Field2
        {
            public string key1 { get; set; }
            public int[] key2 { get; set; }
        }

        public class Nested_Array
        {
            public string inner1 { get; set; }
            public int[] inner2 { get; set; }
            public float inner3 { get; set; }
            public Inner4 inner4 { get; set; }
        }

        public class Inner4
        {
            public string deep1 { get; set; }
            public int[] deep2 { get; set; }
        }

        public class Field10
        {
            public string nested_field1 { get; set; }
            public int[] nested_field2 { get; set; }
            public Nested_Field3 nested_field3 { get; set; }
        }

        public class Nested_Field3
        {
            public string key1 { get; set; }
            public int[] key2 { get; set; }
        }

        public class Nested_Object_Field
        {
            public int[] nested_field1 { get; set; }
            public Nested_Field21 nested_field2 { get; set; }
        }

        public class Nested_Field21
        {
            public string key1 { get; set; }
            public int[] key2 { get; set; }
        }

        public class Array_Of_Objects
        {
            public string item1 { get; set; }
            public int item2 { get; set; }
        }

        public class Nested_Array_Of_Objects
        {
            public string item { get; set; }
        }

        [TestMethod]
        public void RealWorldBigJson()
        {
            string json = @"
	  {
              ""simple_field1"": 42,
              ""simple_field2"": 3.14,
              ""simple_field3"": ""Hello, World!"",
              ""array_of_integers"": [1, 2, 3, 4, 5],
              ""object_field1"": {
                ""subfield1"": 7,
                ""subfield2"": ""Nested string"",
                ""subfield3"": [9, 8]
              },
              ""object_with_fields1"": {
                ""field1"": 123,
                ""field2"": 7.77,
                ""field3"": ""Simple String"",
                ""array_of_floats"": [11.1, 22.2, 33.3],
                ""nested_object"": {
                  ""subfield1"": 77,
                  ""subfield2"": ""Nested string"",
                  ""subfield3"": [99, 88.1]
                },
                ""nested_array"": [
                  {""inner1"": ""value1"", ""inner2"": [1, 2, 3]},
                  {""inner3"": 4.56, ""inner4"": {""deep1"": ""abc"", ""deep2"": [5, 6]}}
                ],
                ""field7"": ""Another field"",
                ""field8"": 555,
                ""field9"": {""nested_field1"": [11, 22, 33], ""nested_field2"": {""key1"": ""value1"", ""key2"": [44, 55]}}
              },
              ""simple_field4"": 789,
              ""simple_field5"": 2.718,
              ""simple_field6"": ""Goodbye, World!"",
              ""array_of_objects"": [
                {""item1"": ""value1"", ""item2"": 42},
                {""item1"": ""value2"", ""item2"": 43},
                {""item1"": ""value3"", ""item2"": 44}
              ],
              ""field10"": {
                ""nested_field1"": ""Value"",
                ""nested_field2"": [10, 20, 30],
                ""nested_field3"": {""key1"": ""value1"", ""key2"": [40, 50]}
              },
              ""field11"": 987,
              ""field12"": 6.626e-34,
              ""field13"": ""Final string"",
              ""array_of_strings"": [""one"", ""two"", ""three"", ""four"", ""five""],
              ""nested_object_field"": {
                ""nested_field1"": [1, 2, 3],
                ""nested_field2"": {""key1"": ""value1"", ""key2"": [4, 5]}
              },
              ""simple_field7"": 999,
              ""simple_field8"": 1.618,
              ""simple_field9"": ""Last field"",
              ""nested_array_of_objects"": [
                {""item"": ""value1""},
                {""item"": ""value2""},
                {""item"": ""value3""}
              ]
            }
		";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam2)parser.Parse( json );

        }
    }
}
