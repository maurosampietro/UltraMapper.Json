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
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Simples.Count() == 5 );
            Assert.IsTrue( result.Simples[ 0 ].Value == "100" );
            Assert.IsTrue( result.Simples[ 1 ].Value == "200" );
            Assert.IsTrue( result.Simples[ 2 ].Value == "300" );
            Assert.IsTrue( result.Simples[ 3 ].Value == "400" );
            Assert.IsTrue( result.Simples[ 4 ].Value == "500" );
        }

        [TestMethod]
        public void QuotationContainsSpecialChars()
        {
            string json = @"{ ""param"":""}{\\][\"",:""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
            Assert.IsTrue( param.Value == @"}{\]["",:" );
        }

        [TestMethod]
        public void QuotationContainsControlChars()
        {
            string json = @"{""param"":""\\\""\b\f\n\r\t""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
            Assert.IsTrue( param.Value == "\\\"\b\f\n\r\t" );
        }

        [TestMethod]
        public void QuotationContainsUnicodeChars()
        {
            string json = @"{""param"":""\u0030\u0031""}";
            json = Mangle( json );

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            var param = (SimpleParam)result.SubParams[ 0 ];

            Assert.IsTrue( param.Name.Equals( "param", _paramNamecomparisonMode ) );
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
            Assert.IsTrue( result.Name.Equals( String.Empty ) );
            Assert.IsTrue( result.SubParams.Count == 0 );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 1 );
            Assert.IsTrue( result.SubParams[ 0 ].Name.Equals( "emptyObject", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((ComplexParam)result.SubParams[ 0 ]).SubParams.Count == 0 );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 1 );
            Assert.IsTrue( result.SubParams[ 0 ].Name.Equals( "emptyObject", _paramNamecomparisonMode ) );
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
            Assert.IsTrue( result.Count() == 0 );
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
            var result = (ComplexParam)parser.Parse( json );

            var subArrayParam = (ArrayParam)result.SubParams[ 0 ];

            Assert.IsTrue( subArrayParam != null );
            Assert.IsTrue( subArrayParam.Name.Equals( "emptyArray", _paramNamecomparisonMode ) );
            Assert.IsTrue( subArrayParam.Count() == 0 );
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

            Assert.IsTrue( result.Arrays.Count == 3 );

            var item1 = result.Arrays[ 0 ];
            Assert.IsTrue( item1.Simples.Count == 2 );
            Assert.IsTrue( item1.Simples[ 0 ].Value == "1" );
            Assert.IsTrue( item1.Simples[ 1 ].Value == "2" );

            var item2 = result.Arrays[ 1 ];
            Assert.IsTrue( item2.Simples.Count == 2 );
            Assert.IsTrue( item2.Simples[ 0 ].Value == "3" );
            Assert.IsTrue( item2.Simples[ 1 ].Value == "4" );

            var item3 = result.Arrays[ 2 ];
            Assert.IsTrue( item3.Simples.Count == 2 );
            Assert.IsTrue( item3.Simples[ 0 ].Value == "5" );
            Assert.IsTrue( item3.Simples[ 1 ].Value == "6" );
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

            Assert.IsTrue( result.Arrays.Count == 3 );

            var item1 = result.Arrays[ 0 ];
            Assert.IsTrue( item1.Simples.Count == 2 );
            Assert.IsTrue( item1.Simples[ 0 ].Value == "1" );
            Assert.IsTrue( item1.Simples[ 1 ].Value == "2" );

            var item2 = result.Arrays[ 1 ];
            Assert.IsTrue( item2.Simples.Count == 2 );
            Assert.IsTrue( item2.Simples[ 0 ].Value == "3" );
            Assert.IsTrue( item2.Simples[ 1 ].Value == "4" );

            var item3 = result.Arrays[ 2 ];
            Assert.IsTrue( item3.Simples.Count == 2 );
            Assert.IsTrue( item3.Simples[ 0 ].Value == "5" );
            Assert.IsTrue( item3.Simples[ 1 ].Value == "6" );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 2 );

            Assert.IsTrue( result.Simples[ 0 ].Name.Equals( "name", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simples[ 0 ].Value == "blogger" );

            var userArray = result.Arrays[ 0 ];
            Assert.IsTrue( userArray.Name.Equals( "users", _paramNamecomparisonMode ) );
            Assert.IsTrue( userArray.Count() == 2 );

            var subArray1 = userArray.Arrays[ 0 ];
            Assert.IsTrue( subArray1.Simples.Count() == 4 );
            Assert.IsTrue( subArray1.Simples[ 0 ].Value == "admins" );
            Assert.IsTrue( subArray1.Simples[ 1 ].Value == "1" );
            Assert.IsTrue( subArray1.Simples[ 2 ].Value == "2" );
            Assert.IsTrue( subArray1.Simples[ 3 ].Value == "3" );

            var subArray2 = userArray.Arrays[ 1 ];
            Assert.IsTrue( subArray2.Count() == 4 );
            Assert.IsTrue( subArray2.Simples[ 0 ].Value == "editors" );
            Assert.IsTrue( subArray2.Simples[ 1 ].Value == "4" );
            Assert.IsTrue( subArray2.Simples[ 2 ].Value == "5" );
            Assert.IsTrue( subArray2.Simples[ 3 ].Value == "6" );
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
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Count() == 2 );

            var item1 = result.Complex[ 0 ];
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 0 ]).Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 0 ]).Value == "red" );

            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 1 ]).Name.Equals( "value", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)item1.SubParams[ 1 ]).Value == "#f00" );

            var item2 = result.Complex[ 1 ];
            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 0 ]).Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 0 ]).Value == "green" );

            Assert.IsTrue( ((SimpleParam)item2.SubParams[ 1 ]).Name.Equals( "value", _paramNamecomparisonMode ) );
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

            Assert.IsTrue( result.Name.Equals( String.Empty, _paramNamecomparisonMode ) );
            Assert.IsTrue( result.SubParams.Count == 5 );

            var param1 = (SimpleParam)result.SubParams[ 0 ];
            Assert.IsTrue( param1.Name.Equals( "unquotedParam", _paramNamecomparisonMode ) );
            Assert.IsTrue( param1.Value == "unquotedValue" );

            var param2 = (SimpleParam)result.SubParams[ 1 ];
            Assert.IsTrue( param2.Name.Equals( "quotedParam", _paramNamecomparisonMode ) );
            Assert.IsTrue( param2.Value == "unquotedValue" );

            var param3 = (SimpleParam)result.SubParams[ 2 ];
            Assert.IsTrue( param3.Name.Equals( "quotedParam2", _paramNamecomparisonMode ) );
            Assert.IsTrue( param3.Value == "quotedValue" );

            var param4 = (SimpleParam)result.SubParams[ 3 ];
            Assert.IsTrue( param4.Name.Equals( "escapedComma", _paramNamecomparisonMode ) );
            Assert.IsTrue( param4.Value == "," );

            var param5 = (SimpleParam)result.SubParams[ 4 ];
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
            var result = (ComplexParam)parser.Parse( json );

            var param1 = result.Simples[ 0 ];
            var param2 = result.Complex[ 0 ];

            Assert.IsTrue( param1.Name == "color" );
            Assert.IsTrue( param1.Value == "red" );
            Assert.IsTrue( param2.Simples[ 0 ].Name == "name" );
            Assert.IsTrue( param2.Simples[ 0 ].Value == "Robert" );
            Assert.IsTrue( param2.Simples[ 1 ].Name == "age" );
            Assert.IsTrue( param2.Simples[ 1 ].Value == "32" );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( result.SubParams.Count == 4 );

            Assert.IsTrue( result.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simples[ 0 ].Value == "0001" );

            Assert.IsTrue( result.Simples[ 1 ].Name.Equals( "ppu", _paramNamecomparisonMode ) );
            Assert.IsTrue( result.Simples[ 1 ].Value == "55" );

            var complexParam = result.Complex[ 0 ];
            Assert.IsTrue( complexParam.SubParams.Count == 1 );
            Assert.IsTrue( complexParam.Name.Equals( "batters", _paramNamecomparisonMode ) );

            var subArray = complexParam.Arrays[ 0 ];
            Assert.IsTrue( subArray.Count() == 2 );
            Assert.IsTrue( subArray.Name.Equals( "batter", _paramNamecomparisonMode ) );

            var complexSubArrayItem1 = subArray.Complex[ 0 ];
            Assert.IsTrue( complexSubArrayItem1.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simples[ 0 ].Value == "1001" );

            Assert.IsTrue( complexSubArrayItem1.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simples[ 1 ].Value == "Regular" );

            var complexSubArrayItem2 = subArray.Complex[ 1 ];
            Assert.IsTrue( complexSubArrayItem2.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simples[ 0 ].Value == "1002" );

            Assert.IsTrue( complexSubArrayItem2.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simples[ 1 ].Value == "Chocolate" );

            var array = result.Arrays[ 0 ];
            Assert.IsTrue( array.Count() == 2 );
            Assert.IsTrue( array.Name.Equals( "toppings", _paramNamecomparisonMode ) );

            var complexArrayItem1 = array.Complex[ 0 ];
            Assert.IsTrue( complexArrayItem1.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simples[ 0 ].Value == "5001" );

            Assert.IsTrue( complexArrayItem1.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simples[ 1 ].Value == "None" );

            var complexArrayItem2 = array.Complex[ 1 ];
            Assert.IsTrue( complexArrayItem2.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simples[ 0 ].Value == "5002" );

            Assert.IsTrue( complexArrayItem2.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simples[ 1 ].Value == "Glazed" );
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
            var result = (ArrayParam)parser.Parse( json );

            Assert.IsTrue( result.Count() == 1 );

            var complexParam = result.Complex[ 0 ];
            Assert.IsTrue( complexParam.SubParams.Count == 4 );

            Assert.IsTrue( complexParam.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexParam.Simples[ 0 ].Value == "0003" );

            Assert.IsTrue( complexParam.Simples[ 1 ].Name.Equals( "ppu", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexParam.Simples[ 1 ].Value == "55" );

            var subComplexParam = complexParam.Complex[ 0 ];
            Assert.IsTrue( subComplexParam.Name.Equals( "batters", _paramNamecomparisonMode ) );

            var subArray = subComplexParam.Arrays[ 0 ];
            Assert.IsTrue( subArray.Complex.Count == 2 );
            Assert.IsTrue( subArray.Name.Equals( "batter", _paramNamecomparisonMode ) );

            var complexSubArrayItem1 = subArray.Complex[ 0 ];
            Assert.IsTrue( complexSubArrayItem1.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simples[ 0 ].Value == "1001" );

            Assert.IsTrue( complexSubArrayItem1.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem1.Simples[ 1 ].Value == "Regular" );

            var complexSubArrayItem2 = subArray.Complex[ 1 ];
            Assert.IsTrue( complexSubArrayItem2.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simples[ 0 ].Value == "1002" );

            Assert.IsTrue( complexSubArrayItem2.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexSubArrayItem2.Simples[ 1 ].Value == "Chocolate" );

            var subArray2 = complexParam.Arrays[ 0 ];
            Assert.IsTrue( subArray2.Complex.Count == 2 );
            Assert.IsTrue( subArray2.Name.Equals( "toppings", _paramNamecomparisonMode ) );

            var complexArrayItem1 = subArray2.Complex[ 0 ];
            Assert.IsTrue( complexArrayItem1.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simples[ 0 ].Value == "5001" );

            Assert.IsTrue( complexArrayItem1.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem1.Simples[ 1 ].Value == "None" );

            var complexArrayItem2 = subArray2.Complex[ 1 ];
            Assert.IsTrue( complexArrayItem2.Simples[ 0 ].Name.Equals( "id", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simples[ 0 ].Value == "5002" );

            Assert.IsTrue( complexArrayItem2.Simples[ 1 ].Name.Equals( "type", _paramNamecomparisonMode ) );
            Assert.IsTrue( complexArrayItem2.Simples[ 1 ].Value == "Glazed" );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Name.Equals( "isParam1Set", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Value == "true" );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Name.Equals( "isParam2Set", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Value == "false" );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 2 ]).Name.Equals( "quotedTrue", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 2 ]).Value == "true" );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 3 ]).Name.Equals( "quotedFalse", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 3 ]).Value == "false" );
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
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Name.Equals( "color", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Value == null );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Name.Equals( "value", _paramNamecomparisonMode ) );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Value == "null" );
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
            var result = (ComplexParam)parser.Parse( json );

            var colorsArray = (ArrayParam)result.SubParams[ 0 ];

            Assert.IsTrue( colorsArray.Name.Equals( "colors", _paramNamecomparisonMode ) );
            Assert.IsTrue( colorsArray.Simples[ 0 ].Value == "red" );
            Assert.IsTrue( colorsArray.Simples[ 1 ].Value == "green" );
            Assert.IsTrue( colorsArray.Simples[ 2 ].Value == null );
            Assert.IsTrue( colorsArray.Simples[ 3 ].Value == "yellow" );
        }
    }
}
