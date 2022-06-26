using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Json.Tests.MapperTests
{
    [TestCategory( "Mapper tests" )]
    [TestClass]
    public class JsonWriter
    {
        private class CircularReferenceObject
        {
            public int Simple1 { get; set; }
            public string Simple2 { get; set; }
            public double Simple3 { get; set; }

            public CircularReferenceObject Complex { get; set; }
        }

        private class ClassB
        {
            public int Simple1 { get; set; }
            public string Simple2 { get; set; }
            public double Simple3 { get; set; }
        }

        private class ClassA
        {
            public int Simple1 { get; set; }
            public string Simple2 { get; set; }
            public double Simple3 { get; set; }

            public ClassB ObjB { get; set; }
        }

        [TestMethod]
        public void CircularReference()
        {
            var objToJson = new CircularReferenceObject()
            {
                Simple1 = 11,
                Simple2 = "ciao",
                Simple3 = 13.0,
            };

            objToJson.Complex = new CircularReferenceObject()
            {
                Simple1 = 21,
                Simple2 = "hello",
                Simple3 = 23.0,
                Complex = objToJson
            };

            var parser = new JsonSerializer();
            var outputJson = parser.Serialize( objToJson );

            var jsonToObj = parser.Deserialize<CircularReferenceObject>( outputJson );

            Assert.IsTrue( jsonToObj.Simple1 == objToJson.Simple1 );
            Assert.IsTrue( jsonToObj.Simple2 == objToJson.Simple2 );
            Assert.IsTrue( jsonToObj.Simple3 == objToJson.Simple3 );

            Assert.IsTrue( jsonToObj.Complex.Simple1 == objToJson.Complex.Simple1 );
            Assert.IsTrue( jsonToObj.Complex.Simple2 == objToJson.Complex.Simple2 );
            Assert.IsTrue( jsonToObj.Complex.Simple3 == objToJson.Complex.Simple3 );
        }

        [TestMethod]
        public void Test()
        {
            var objToJson = new ClassA()
            {
                Simple1 = 11,
                Simple2 = "ciao",
                Simple3 = 13.0,

                ObjB = new ClassB()
                {
                    Simple1 = 21,
                    Simple2 = "hello",
                    Simple3 = 23.0
                }
            };

            var parser = new JsonSerializer();
            var json = parser.Serialize( objToJson );

            var jsonToObj = parser.Deserialize<ClassA>( json );

            Assert.IsTrue( jsonToObj.Simple1 == objToJson.Simple1 );
            Assert.IsTrue( jsonToObj.Simple2 == objToJson.Simple2 );
            Assert.IsTrue( jsonToObj.Simple3 == objToJson.Simple3 );

            Assert.IsTrue( jsonToObj.ObjB.Simple1 == objToJson.ObjB.Simple1 );
            Assert.IsTrue( jsonToObj.ObjB.Simple2 == objToJson.ObjB.Simple2 );
            Assert.IsTrue( jsonToObj.ObjB.Simple3 == objToJson.ObjB.Simple3 );
        }
    }
}
