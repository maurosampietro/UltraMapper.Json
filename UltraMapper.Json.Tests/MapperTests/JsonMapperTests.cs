﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using UltraMapper.Parsing;

namespace UltraMapper.Json.Tests.MapperTests
{
    [TestCategory( "Mapper deserializing tests" )]
    [TestClass]
    public class JsonMapperTests
    {
        [TestMethod]
        public void EmptyArray()
        {
            string inputJson = "[]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int[]>( inputJson );

            Assert.IsTrue( result.Length == 0 );
        }

        [TestMethod]
        public void EmptyArrayOnNonCollection()
        {
            string inputJson = "[]";

            var parser = new JsonSerializer();

            //should throw some specific exception: cannot deserialize array on non-collection
            Assert.ThrowsException<Exception>(
                () => parser.Deserialize<object>( inputJson ) );
        }

        [TestMethod]
        public void ArrayPrimitiveType()
        {
            string inputJson = "[ 100, 200, 300, 400, 500 ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int[]>( inputJson );

            Assert.IsTrue( result.Length == 5 );
            Assert.IsTrue( result[ 0 ] == 100 );
            Assert.IsTrue( result[ 1 ] == 200 );
            Assert.IsTrue( result[ 2 ] == 300 );
            Assert.IsTrue( result[ 3 ] == 400 );
            Assert.IsTrue( result[ 4 ] == 500 );
        }

        [TestMethod]
        public void ArrayPrimitiveTypeNullItemToNonNullableArray()
        {
            //We expected the default value because we deserialize to non-nullable int[]

            string inputJson = "[ null, 100, 200 ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int[]>( inputJson );

            Assert.IsTrue( result.Length == 3 );
            Assert.IsTrue( result[ 0 ] == 0 ); //defaults to 0
            Assert.IsTrue( result[ 1 ] == 100 );
            Assert.IsTrue( result[ 2 ] == 200 );
        }

        [TestMethod]
        public void ArrayPrimitiveTypeNullItemToNullableArray()
        {
            //We expect null because we deserialize to nullable int?[]

            string inputJson = "[ null, 100, 200 ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int?[]>( inputJson );

            Assert.IsTrue( result.Length == 3 );
            Assert.IsTrue( result[ 0 ] == null );
            Assert.IsTrue( result[ 1 ] == 100 );
            Assert.IsTrue( result[ 2 ] == 200 );
        }

        [TestMethod]
        public void MultiDimensionalArrayPrimitiveType()
        {
            string inputJson = "[ [100,101], [200,201], [300,301] ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int[][]>( inputJson );

            Assert.IsTrue( result.Length == 3 );
            Assert.IsTrue( result[ 0 ][ 0 ] == 100 );
            Assert.IsTrue( result[ 0 ][ 1 ] == 101 );
            Assert.IsTrue( result[ 1 ][ 0 ] == 200 );
            Assert.IsTrue( result[ 1 ][ 1 ] == 201 );
            Assert.IsTrue( result[ 2 ][ 0 ] == 300 );
            Assert.IsTrue( result[ 2 ][ 1 ] == 301 );
        }

        [TestMethod]
        public void MultiDimensionalArrayPrimitiveTypeNullItem()
        {
            string inputJson = "[ null, [200,201], [300,301] ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<int[][]>( inputJson );

            Assert.IsTrue( result.Length == 3 );
            Assert.IsTrue( result[ 0 ] == null );
            Assert.IsTrue( result[ 1 ][ 0 ] == 200 );
            Assert.IsTrue( result[ 1 ][ 1 ] == 201 );
            Assert.IsTrue( result[ 2 ][ 0 ] == 300 );
            Assert.IsTrue( result[ 2 ][ 1 ] == 301 );
        }

        [TestMethod]
        public void ListPrimitiveType()
        {
            string inputJson = "[ 100, 200, 300 ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<List<int>>( inputJson );

            Assert.IsTrue( result.Count == 3 );
            Assert.IsTrue( result[ 0 ] == 100 );
            Assert.IsTrue( result[ 1 ] == 200 );
            Assert.IsTrue( result[ 2 ] == 300 );
        }

        public class ColorValue
        {
            public string Color { get; set; }
            public string Value { get; set; }
        }

        [TestMethod]
        public void ArrayOfComplexObject()
        {
            string inputJson = @"
		    [
			    {
				    value: ""#f00"",
                    color: ""red"",			
			    },
			    {
				    color: ""green"",
				    value: ""#0f0""
			    },
		    ]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<List<ColorValue>>( inputJson );

            Assert.IsTrue( result.Count == 2 );

            Assert.IsTrue( result[ 0 ].Color == "red" );
            Assert.IsTrue( result[ 0 ].Value == "#f00" );

            Assert.IsTrue( result[ 1 ].Color == "green" );
            Assert.IsTrue( result[ 1 ].Value == "#0f0" );
        }

        private class Example3Class
        {
            public string UnquotedParam { get; set; }
            public string QuotedParam { get; set; }
            public string QuotedParam2 { get; set; }
            public string EscapedComma { get; set; }
        }

        [TestMethod]
        public void QuotedAndUnquotedParamsAndValues()
        {
            string inputJson = @" 
			{
				unquotedParam: unquotedValue,
				""quotedParam"": unquotedValue,
				""quotedParam2"": ""quotedValue"",
				escapedComma:"",""
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Example3Class>( inputJson );

            Assert.IsTrue( result.UnquotedParam == "unquotedValue" );
            Assert.IsTrue( result.QuotedParam == "unquotedValue" );
            Assert.IsTrue( result.QuotedParam2 == "quotedValue" );
            Assert.IsTrue( result.EscapedComma == "," );
        }

        private class Example3ClassSubobject
        {
            public class UserInfo
            {
                public string Name { get; set; }
                public string Age { get; set; }
            }

            public string Color { get; set; }
            public UserInfo User { get; set; }
        }

        [TestMethod]
        public void ObjectWithSubobject()
        {
            string inputJson = @" 
			{
				color : ""red"",
				user :
				{
					name:Robert,
					age:32
				}
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Example3ClassSubobject>( inputJson );

            Assert.IsTrue( result.Color == "red" );
            Assert.IsTrue( result.User.Name == "Robert" );
            Assert.IsTrue( result.User.Age == "32" );
        }

        //     [TestMethod]
        //     public void Example5ArrayOfHighlyNestedComplexObjects()
        //     {
        //         string inputJson = @"
        //[
        //	{
        //		""id"": ""0003"",
        //		""ppu"": 0.55,

        //                 ""batters"":
        //		{
        //			""batter"":
        //			[
        //				{ ""id"": ""1001"", ""type"": ""Regular"" },
        //				{ ""id"": ""1002"", ""type"": ""Chocolate"" }
        //			]
        //		},

        //                 ""toppings"":
        //		[
        //			{ ""id"": ""5001"", ""type"": ""None"" },
        //			{ ""id"": ""5002"", ""type"": ""Glazed"" },
        //		]
        //	}
        //]";

        //         var parser = new JsonParser();
        //         var result = (ArrayParam)parser.Parse( inputJson );

        //         Assert.IsTrue( result.Items.Count == 1 );

        //         var complexParam = (ComplexParam)result.Items[ 0 ];
        //         Assert.IsTrue( complexParam.SubParams.Length == 4 );

        //         Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 0 ]).Name == "id" );
        //         Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 0 ]).Value == "0003" );

        //         Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 1 ]).Name == "ppu" );
        //         Assert.IsTrue( ((SimpleParam)complexParam.SubParams[ 1 ]).Value == "0.55" );

        //         var subComplexParam = (ComplexParam)complexParam.SubParams[ 2 ];
        //         Assert.IsTrue( subComplexParam.Name == "batters" );

        //         var subArray = (ArrayParam)subComplexParam.SubParams[ 0 ];
        //         Assert.IsTrue( subArray.Items.Count == 2 );
        //         Assert.IsTrue( subArray.Name == "batter" );

        //         var complexSubArrayItem1 = (ComplexParam)subArray.Items[ 0 ];
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Name == "id" );
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 0 ]).Value == "1001" );

        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Name == "type" );
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem1.SubParams[ 1 ]).Value == "Regular" );

        //         var complexSubArrayItem2 = (ComplexParam)subArray.Items[ 1 ];
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Name == "id" );
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 0 ]).Value == "1002" );

        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Name == "type" );
        //         Assert.IsTrue( ((SimpleParam)complexSubArrayItem2.SubParams[ 1 ]).Value == "Chocolate" );

        //         var subArray2 = (ArrayParam)complexParam.SubParams[ 3 ];
        //         Assert.IsTrue( subArray2.Items.Count == 2 );
        //         Assert.IsTrue( subArray2.Name == "toppings" );

        //         var complexArrayItem1 = (ComplexParam)subArray2.Items[ 0 ];
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Name == "id" );
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 0 ]).Value == "5001" );

        //         Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Name == "type" );
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem1.SubParams[ 1 ]).Value == "None" );

        //         var complexArrayItem2 = (ComplexParam)subArray2.Items[ 1 ];
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Name == "id" );
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 0 ]).Value == "5002" );

        //         Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Name == "type" );
        //         Assert.IsTrue( ((SimpleParam)complexArrayItem2.SubParams[ 1 ]).Value == "Glazed" );
        //     }

        //     [TestMethod]
        //     public void QuotationContainsSpecialChars()
        //     {
        //         string inputJson = @"{ param:""}{\\][\"",:""}";

        //         var parser = new JsonParser();
        //         var result = (ComplexParam)parser.Parse( inputJson );

        //         var param = (SimpleParam)result.SubParams[ 0 ];

        //         Assert.IsTrue( param.Name == "param" );
        //         Assert.IsTrue( param.Value == @"}{\]["",:" );
        //     }

        //     [TestMethod]
        //     public void QuotationContainsControlChars()
        //     {
        //         string inputJson = @"{param:""\""\\\/\b\f\n\r\t""}";

        //         var parser = new JsonParser();
        //         var result = (ComplexParam)parser.Parse( inputJson );

        //         var param = (SimpleParam)result.SubParams[ 0 ];

        //         Assert.IsTrue( param.Name == "param" );
        //         Assert.IsTrue( param.Value == "\"\\/\b\f\n\r\t" );
        //     }

        //     [TestMethod]
        //     public void QuotationContainsUnicodeChars()
        //     {
        //         string inputJson = @"{param:""\u0030\u0031""}";

        //         var parser = new JsonParser();
        //         var result = (ComplexParam)parser.Parse( inputJson );

        //         var param = (SimpleParam)result.SubParams[ 0 ];

        //         Assert.IsTrue( param.Name == "param" );
        //         Assert.IsTrue( param.Value == "\u0030\u0031" );
        //     }

        //[TestMethod]
        //public void EmptyObject()
        //{
        //    string inputJson = @"{}";

        //    var parser = new JsonParser();
        //    var result = (ComplexParam)parser.Parse( inputJson );

        //    Assert.IsTrue( result != null );
        //    Assert.IsTrue( result.Name == String.Empty );
        //    Assert.IsTrue( result.SubParams == null );
        //}

        //[TestMethod]
        //public void EmptySubObject()
        //{
        //    string inputJson = @"
        //    { 
        //        emptyObject : {}
        //    }";

        //    var parser = new JsonParser();
        //    var result = (ComplexParam)parser.Parse( inputJson );
        //}

        //[TestMethod]
        //public void EmptyArray()
        //{
        //    string inputJson = @"[]";

        //    var parser = new JsonParserSerializer();
        //    var result = parser.Deserialize<Example3ClassSubobject>( inputJson );

        //    Assert.IsTrue( result != null );
        //    Assert.IsTrue( result.Items.Count == 0 );
        //}

        //[TestMethod]
        //public void EmptySubArray()
        //{
        //    string inputJson = @"
        //    { 
        //        emptyArray : []
        //    }";

        //    var parser = new JsonParserSerializer();
        //    var result = parser.Deserialize<Example3ClassSubobject>( inputJson );

        //    Assert.IsTrue( arrayParam != null );
        //    Assert.IsTrue( arrayParam.Items.Count == 0 );
        //}

        //[TestMethod]
        //public void SubArrays()
        //{
        //    string inputJson = @"
        //    [
        //        [1,2], [3,4], [5,6]
        //    ]";

        //    var parser = new JsonParser();
        //    var result = (ArrayParam)parser.Parse( inputJson );

        //    Assert.IsTrue( result.Items.Count == 3 );

        //    var item1 = (ArrayParam)result[ 0 ];
        //    Assert.IsTrue( item1.Items.Count == 2 );
        //    Assert.IsTrue( ((SimpleParam)item1[ 0 ]).Value == "1" );
        //    Assert.IsTrue( ((SimpleParam)item1[ 1 ]).Value == "2" );

        //    var item2 = (ArrayParam)result[ 1 ];
        //    Assert.IsTrue( item2.Items.Count == 2 );
        //    Assert.IsTrue( ((SimpleParam)item2[ 0 ]).Value == "3" );
        //    Assert.IsTrue( ((SimpleParam)item2[ 1 ]).Value == "4" );

        //    var item3 = (ArrayParam)result[ 2 ];
        //    Assert.IsTrue( item3.Items.Count == 2 );
        //    Assert.IsTrue( ((SimpleParam)item3[ 0 ]).Value == "5" );
        //    Assert.IsTrue( ((SimpleParam)item3[ 1 ]).Value == "6" );
        //}
    }

    [TestCategory( "Mapper deserializing tests" )]
    [TestClass]
    public class Example4
    {
        public class Item
        {
            public string Id { get; set; }
            public string Ppu { get; set; }
            public Batters Batters { get; set; }
            public Ingredient[] Toppings { get; set; }
        }

        public class Batters
        {
            public List<Ingredient> Batter { get; set; }
        }

        public class Ingredient
        {
            public string Id { get; set; }
            public string Type { get; set; }
        }

        [TestMethod]
        public void HighlyNestedComplexObject()
        {
            string inputJson = @"
			{
				""id"": ""0001"",
				""ppu"": 0.55	,

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

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Item>( inputJson );

            Assert.IsTrue( result.Id == "0001" );
            Assert.IsTrue( result.Ppu == "0.55" );

            Assert.IsTrue( result.Batters.Batter.Count == 2 );

            var bItem0 = result.Batters.Batter[ 0 ];
            Assert.IsTrue( bItem0.Id == "1001" );
            Assert.IsTrue( bItem0.Type == "Regular" );

            var bItem1 = result.Batters.Batter[ 1 ];
            Assert.IsTrue( bItem1.Id == "1002" );
            Assert.IsTrue( bItem1.Type == "Chocolate" );

            Assert.IsTrue( result.Toppings.Length == 2 );
            var tItem0 = result.Toppings[ 0 ];
            Assert.IsTrue( tItem0.Id == "5001" );
            Assert.IsTrue( tItem0.Type == "None" );

            var tItem1 = result.Toppings[ 1 ];
            Assert.IsTrue( tItem1.Id == "5002" );
            Assert.IsTrue( tItem1.Type == "Glazed" );
        }

        [TestMethod]
        public void SetParamToNullComplexParam()
        {
            string inputJson = @"
			{
				""id"": ""0001"",
				""ppu"": 0.55	,

				""batters"": null		
				""toppings"": null
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Item>( inputJson );

            Assert.IsTrue( result.Id == "0001" );
            Assert.IsTrue( result.Ppu == "0.55" );

            Assert.IsTrue( result.Batters == null );
            Assert.IsTrue( result.Toppings == null );
        }

        [TestMethod]
        public void SetArrayParamToNull()
        {
            string inputJson = @"
			{
				""id"": 0001,
				""ppu"": 0.55,

				""batters"":
				{
					""batter"":null,
				},
				
				""toppings"": null
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Item>( inputJson );

            Assert.IsTrue( result.Id == "0001" );
            Assert.IsTrue( result.Ppu == "0.55" );

            Assert.IsTrue( result.Batters.Batter == null );
            Assert.IsTrue( result.Toppings == null );
        }

        [TestMethod]
        public void SetComplexParamArrayItemToNull()
        {
            string inputJson = @"
			{				
                ""id"": ""0001"",
                ""ppu"": 0.55	,

                ""batters"":
                {
            	    ""batter"":
            	    [
            		    null,
            		    { ""id"": ""1002"", ""type"": ""Chocolate"" },
            	    ]
                },

				""toppings"":
				[
					{ ""id"": ""5001"", ""type"": ""None"" },
					null,
				]
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Item>( inputJson );

            Assert.IsTrue( result.Id == "0001" );
            Assert.IsTrue( result.Ppu == "0.55" );

            Assert.IsTrue( result.Batters.Batter.Count == 2 );
            Assert.IsTrue( result.Batters.Batter[ 0 ] == null );
            Assert.IsTrue( result.Batters.Batter[ 1 ].Id == "1002" );
            Assert.IsTrue( result.Batters.Batter[ 1 ].Type == "Chocolate" );

            Assert.IsTrue( result.Toppings.Length == 2 );
            Assert.IsTrue( result.Toppings[ 0 ].Id == "5001" );
            Assert.IsTrue( result.Toppings[ 0 ].Type == "None" );
            Assert.IsTrue( result.Toppings[ 1 ] == null );
        }
    }

    [TestCategory( "Mapper deserializing tests" )]
    [TestClass]
    public class Example5
    {
        public class Commands2
        {
            public class InnerType
            {
                public string A { get; set; }
                public string B { get; set; }
                public InnerType Inner2 { get; set; }
            }

            public InnerType Move { get; set; }
        }

        [TestMethod]
        public void CircularReferenceWhenReading()
        {
            var json = @"
            {
                move:
                {
                    a : a,
                    b : b,
                    inner2:
                    {
                        a:aa,
                        b:bb,
                        inner2:
                        {
                            a:aaa,
                            b:bbb,
                            inner2:
                            {
                                a:aaaa, 
                                b:bbbb
                            }
                        }
                    }
                }
            }";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<Commands2>( json );

            Assert.IsTrue( result.Move.A == "a" );
            Assert.IsTrue( result.Move.B == "b" );
            Assert.IsTrue( result.Move.Inner2.A == "aa" );
            Assert.IsTrue( result.Move.Inner2.B == "bb" );
            Assert.IsTrue( result.Move.Inner2.Inner2.A == "aaa" );
            Assert.IsTrue( result.Move.Inner2.Inner2.B == "bbb" );
            Assert.IsTrue( result.Move.Inner2.Inner2.Inner2.A == "aaaa" );
            Assert.IsTrue( result.Move.Inner2.Inner2.Inner2.B == "bbbb" );
        }

        [TestMethod]
        public void SetParamToNullSimpleParam()
        {
            string json = @"
			{
				color: null,
				value: ""null""
			}";

            var parser = new JsonParser();
            var result = (ComplexParam)parser.Parse( json );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Name == "color" );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 0 ]).Value == null );

            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Name == "value" );
            Assert.IsTrue( ((SimpleParam)result.SubParams[ 1 ]).Value == "null" );
        }

        private class BoolTests
        {
            public bool IsParam1Set { get; set; }
            public bool IsParam2Set { get; set; }
            public string QuotedTrue { get; set; }
            public string QuotedFalse { get; set; }
        }

        [TestMethod]
        public void SetParamToBool()
        {
            string json = @"
			{
				isParam1Set: true,
				isParam2Set: false,
	            quotedTrue: ""true"",
				quotedFalse: ""false"",
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<BoolTests>( json );

            Assert.IsTrue( result.IsParam1Set == true );
            Assert.IsTrue( result.IsParam2Set == false );
            Assert.IsTrue( result.QuotedTrue == "true" );
            Assert.IsTrue( result.QuotedFalse == "false" );
        }

        [TestMethod]
        public void SetParamArrayToBool1()
        {
            string json = @"[true, false, true, false]";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<bool[]>( json );

            Assert.IsTrue( result[ 0 ] == true );
            Assert.IsTrue( result[ 1 ] == false );
            Assert.IsTrue( result[ 2 ] == true );
            Assert.IsTrue( result[ 3 ] == false );
        }

        private class BoolArrayTests
        {
            public bool[] BoolArray { get; set; }
        }

        [TestMethod]
        public void SetParamArrayToBool2()
        {
            string json = @"
			{
				boolArray: [true, false, true, false]
			}";

            var parser = new JsonSerializer();
            var result = parser.Deserialize<BoolArrayTests>( json );

            Assert.IsTrue( result.BoolArray[ 0 ] == true );
            Assert.IsTrue( result.BoolArray[ 1 ] == false );
            Assert.IsTrue( result.BoolArray[ 2 ] == true );
            Assert.IsTrue( result.BoolArray[ 3 ] == false );
        }
    }
}
