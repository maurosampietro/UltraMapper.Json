using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UltraMapper.Json.Tests.ParserTests.JsonManglers;
using UltraMapper.Parsing;

namespace UltraMapper.Json.Tests.ParserTests
{
    public class JsonParserTests
    {
        private readonly IJsonMangler[] _manglers;

        public JsonParserTests( params IJsonMangler[] manglers )
        {
            _manglers = manglers ?? new[] { new DoNothingMangler() };
        }

        private string Mangle( string json )
        {
            if( _manglers == null ) return json;

            return _manglers.Aggregate( json,
                ( aggJson, mangler ) => mangler.Mangle( aggJson ) );
        }

        [TestMethod]
        public void Example1ArrayPrimitiveType()
        {
            string json = "[ 100 , 200, 300, 400, 500 ]";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Items.Count == 5 );
            Assert.IsTrue( ((SimpleParam)result.Items[ 0 ]).Value == "100" );
            Assert.IsTrue( ((SimpleParam)result.Items[ 1 ]).Value == "200" );
            Assert.IsTrue( ((SimpleParam)result.Items[ 2 ]).Value == "300" );
            Assert.IsTrue( ((SimpleParam)result.Items[ 3 ]).Value == "400" );
            Assert.IsTrue( ((SimpleParam)result.Items[ 4 ]).Value == "500" );
        }

        [TestMethod]
        public void QuotationContainsSpecialChars()
        {
            string json = @"{ param:""}{\\][\"",:""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name == "param" );
            Assert.IsTrue( param.Value == @"}{\]["",:" );
        }

        [TestMethod]
        public void QuotationContainsControlChars()
        {
            string json = @"{param:""\\\""\b\f\n\r\t""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name == "param" );
            Assert.IsTrue( param.Value == "\\\"\b\f\n\r\t" );
        }

        [TestMethod]
        public void QuotationContainsUnicodeChars()
        {
            string json = @"{param:""\u0030\u0031""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name == "param" );
            Assert.IsTrue( param.Value == "\u0030\u0031" );
        }

        [TestMethod]
        public void EmptyObject()
        {
            string json = @"{}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result != null );
            Assert.IsTrue( result.Name == String.Empty );
            Assert.IsTrue( result.SubParams.Count == 0 );
        }

        [TestMethod]
        public void EmptySubObject()
        {
            string json = @"
			{ 
				emptyObject : {}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 1 );
            Assert.IsTrue( result.SubParams[ 0 ].Name == "emptyObject" );
            Assert.IsTrue( ((ComplexParam)result.SubParams[ 0 ]).SubParams.Count == 0 );
        }

        [TestMethod]
        public void EmptySubObject2()
        {
            string json = @"
			{ 
				emptyObject : {,,}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 1 );
            Assert.IsTrue( result.SubParams[ 0 ].Name == "emptyObject" );
            Assert.IsTrue( ((ComplexParam)result.SubParams[ 0 ]).SubParams.Count == 0 );
        }

        [TestMethod]
        public void EmptyArray()
        {
            string json = @"[]";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result != null );
            Assert.IsTrue( result.Items.Count == 0 );
        }

        [TestMethod]
        public void EmptySubArray()
        {
            string json = @"
			{ 
				emptyArray : []
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var subArrayParam = (ArrayParam)result.SubParams[ 0 ];

            Assert.IsTrue( subArrayParam != null );
            Assert.IsTrue( subArrayParam.Name == "emptyArray" );
            Assert.IsTrue( subArrayParam.Items.Count == 0 );
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
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Items.Count == 3 );

            var item1 = (ArrayParam)result[ 0 ];
            Assert.IsTrue( item1.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item1[ 0 ]).Value == "1" );
            Assert.IsTrue( ((SimpleParam)item1[ 1 ]).Value == "2" );

            var item2 = (ArrayParam)result[ 1 ];
            Assert.IsTrue( item2.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item2[ 0 ]).Value == "3" );
            Assert.IsTrue( ((SimpleParam)item2[ 1 ]).Value == "4" );

            var item3 = (ArrayParam)result[ 2 ];
            Assert.IsTrue( item3.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item3[ 0 ]).Value == "5" );
            Assert.IsTrue( ((SimpleParam)item3[ 1 ]).Value == "6" );
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
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Items.Count == 3 );

            var item1 = (ArrayParam)result[ 0 ];
            Assert.IsTrue( item1.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item1[ 0 ]).Value == "1" );
            Assert.IsTrue( ((SimpleParam)item1[ 1 ]).Value == "2" );

            var item2 = (ArrayParam)result[ 1 ];
            Assert.IsTrue( item2.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item2[ 0 ]).Value == "3" );
            Assert.IsTrue( ((SimpleParam)item2[ 1 ]).Value == "4" );

            var item3 = (ArrayParam)result[ 2 ];
            Assert.IsTrue( item3.Items.Count == 2 );
            Assert.IsTrue( ((SimpleParam)item3[ 0 ]).Value == "5" );
            Assert.IsTrue( ((SimpleParam)item3[ 1 ]).Value == "6" );
        }

        [TestMethod]
        public void Example2ArrayOfComplexObjects()
        {
            string json = @"
			[
				{
					color: ""red"",
					value: ""#f00""
				},
				{
					color: ""green"",
					value: ""#0f0""
				},
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Items.Count == 2 );

            var item1 = (ComplexParam)result.Items[ 0 ];
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 0 ]).Name == "color" );
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 0 ]).Value == "red" );

            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 1 ]).Name == "value" );
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 1 ]).Value == "#f00" );

            var item2 = (ComplexParam)result.Items[ 1 ];
            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 0 ]).Name == "color" );
            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 0 ]).Value == "green" );

            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 1 ]).Name == "value" );
            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 1 ]).Value == "#0f0" );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.Name == String.Empty );
            Assert.IsTrue( result.SubParams.Count == 5 );

            var param1 = (SimpleParam)result.SubParams[ 0 ];
            Assert.IsTrue( param1.Name == "unquotedParam" );
            Assert.IsTrue( param1.Value == "unquotedValue" );

            var param2 = (SimpleParam)result.SubParams[ 1 ];
            Assert.IsTrue( param2.Name == "quotedParam" );
            Assert.IsTrue( param2.Value == "unquotedValue" );

            var param3 = (SimpleParam)result.SubParams[ 2 ];
            Assert.IsTrue( param3.Name == "quotedParam2" );
            Assert.IsTrue( param3.Value == "quotedValue" );

            var param4 = (SimpleParam)result.SubParams[ 3 ];
            Assert.IsTrue( param4.Name == "escapedComma" );
            Assert.IsTrue( param4.Value == "," );

            var param5 = (SimpleParam)result.SubParams[ 4 ];
            Assert.IsTrue( param5.Name == "escapedQuoteAndComma" );
            Assert.IsTrue( param5.Value == "\",\"" );
        }

        [TestMethod]
        public void Example3ObjectWithSubobject()
        {
            string json = @" 
			{
				color: ""red"",
				user:
				{
					name:Robert,
					age:32
				}
			}";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param1 = (SimpleParam)result.SubParams[ 0 ];
            var param2 = (ComplexParam)result.SubParams[ 1 ];

            param1.Name = "color";
            param1.Value = "red";

            ((SimpleParam)param2.SubParams[ 0 ]).Name = "name";
            ((SimpleParam)param2.SubParams[ 0 ]).Value = "Robert";

            ((SimpleParam)param2.SubParams[ 1 ]).Name = "age";
            ((SimpleParam)param2.SubParams[ 1 ]).Value = "32";
        }

        [TestMethod]
        public void Example4HighlyNestedComplexObject()
        {
            string json = @"
			{
				""id"": ""0001"",
				""ppu"": 0.55,
		
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 4 );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Value == "0001" );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Name == "ppu" );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Value == "0.55" );

            var complexParam = (ComplexParam)result.SubParams[ 2 ];
            Assert.IsTrue( complexParam.SubParams.Count == 1 );
            Assert.IsTrue( complexParam.Name == "batters" );

            var subArray = (ArrayParam)complexParam.SubParams[ 0 ];
            Assert.IsTrue( subArray.Items.Count == 2 );
            Assert.IsTrue( subArray.Name == "batter" );

            var complexSubArrayItem1 = (ComplexParam)subArray.Items[ 0 ];
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Value == "1001" );

            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Value == "Regular" );

            var complexSubArrayItem2 = (ComplexParam)subArray.Items[ 1 ];
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Value == "1002" );

            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Value == "Chocolate" );

            var array = (ArrayParam)result.SubParams[ 3 ];
            Assert.IsTrue( array.Items.Count == 2 );
            Assert.IsTrue( array.Name == "toppings" );

            var complexArrayItem1 = (ComplexParam)array.Items[ 0 ];
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Value == "5001" );

            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Value == "None" );

            var complexArrayItem2 = (ComplexParam)array.Items[ 1 ];
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Value == "5002" );

            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Value == "Glazed" );
        }

        [TestMethod]
        public void Example5ArrayOfHighlyNestedComplexObjects()
        {
            string json = @"
			[
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
			]";

            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Items.Count == 1 );

            var complexParam = (ComplexParam)result.Items[ 0 ];
            Assert.IsTrue( complexParam.SubParams.Count == 4 );

            Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 0 ]).Value == "0003" );

            Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 1 ]).Name == "ppu" );
            Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 1 ]).Value == "0.55" );

            var subComplexParam = (ComplexParam)complexParam.SubParams[ 2 ];
            Assert.IsTrue( subComplexParam.Name == "batters" );

            var subArray = (ArrayParam)subComplexParam.SubParams[ 0 ];
            Assert.IsTrue( subArray.Items.Count == 2 );
            Assert.IsTrue( subArray.Name == "batter" );

            var complexSubArrayItem1 = (ComplexParam)subArray.Items[ 0 ];
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Value == "1001" );

            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Value == "Regular" );

            var complexSubArrayItem2 = (ComplexParam)subArray.Items[ 1 ];
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Value == "1002" );

            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Value == "Chocolate" );

            var subArray2 = (ArrayParam)complexParam.SubParams[ 3 ];
            Assert.IsTrue( subArray2.Items.Count == 2 );
            Assert.IsTrue( subArray2.Name == "toppings" );

            var complexArrayItem1 = (ComplexParam)subArray2.Items[ 0 ];
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Value == "5001" );

            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Value == "None" );

            var complexArrayItem2 = (ComplexParam)subArray2.Items[ 1 ];
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Name == "id" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Value == "5002" );

            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Name == "type" );
            Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Value == "Glazed" );
        }
    }
}
