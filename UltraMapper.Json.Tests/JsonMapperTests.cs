using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UltraMapper.Json.Test
{
	[TestClass]
	public class JsonMapperTests
	{
		//[TestMethod]
		//public void Example1ArrayPrimitiveType()
		//{
		//    string inputJson = "[ 100, 200, 300, 400, 500 ]";

		//    var parser = new JsonSerializer();
		//    var result = parser.Deserialize<int[]>( inputJson );

		//    Assert.IsTrue( result.Length == 5 );
		//    Assert.IsTrue( result[ 0 ] == 100 );
		//    Assert.IsTrue( result[ 1 ] == 200 );
		//    Assert.IsTrue( result[ 2 ] == 300 );
		//    Assert.IsTrue( result[ 3 ] == 400 );
		//    Assert.IsTrue( result[ 4 ] == 500 );
		//}

		public class ColorValue
		{
            public string Color { get; set; }
            public string Value { get; set; }
        }

        [TestMethod]
        public void Example2ArrayOfComplexObject()
        {
            string inputJson = @"
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

            var parser = new JsonSerializer();
            var result = parser.Deserialize<List<ColorValue>>( inputJson );

            Assert.IsTrue( result.Count == 2 );

            Assert.IsTrue( result[0].Value =="#f00" );
			Assert.IsTrue( result[ 0 ].Value == "#0f0" );
        }

        private class Example3Class
		{
			public string UnquotedParam { get; set; }
			public string QuotedParam { get; set; }
			public string QuotedParam2 { get; set; }
			public string EscapedComma { get; set; }
		}

		[TestMethod]
		public void MapObject()
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
		public void Example3ObjectWithSubobject()
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

	[TestClass]
	public class Example4
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

		[TestMethod]
		public void Example4HighlyNestedComplexObject()
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

			Assert.IsTrue( result.id == "0001" );
			Assert.IsTrue( result.ppu == "0.55" );

			Assert.IsTrue( result.batters.batter.Count == 2 );

			var bItem0 = result.batters.batter[ 0 ];
			Assert.IsTrue( bItem0.id == "1001" );
			Assert.IsTrue( bItem0.type == "Regular" );

			var bItem1 = result.batters.batter[ 1 ];
			Assert.IsTrue( bItem1.id == "1002" );
			Assert.IsTrue( bItem1.type == "Chocolate" );

			Assert.IsTrue( result.toppings.Length == 2 );
			var tItem0 = result.toppings[ 0 ];
			Assert.IsTrue( tItem0.id == "5001" );
			Assert.IsTrue( tItem0.type == "None" );

			var tItem1 = result.toppings[ 1 ];
			Assert.IsTrue( tItem1.id == "5002" );
			Assert.IsTrue( tItem1.type == "Glazed" );
		}
	}
}
