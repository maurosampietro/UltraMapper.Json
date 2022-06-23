using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Json.Test
{
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
            var obj = new CircularReferenceObject()
            {
                Simple1 = 11,
                Simple2 = "ciao",
                Simple3 = 13.0,
            };

            obj.Complex = new CircularReferenceObject()
            {
                Simple1 = 21,
                Simple2 = "hello",
                Simple3 = 23.0,
                Complex = obj
            };

            var parser = new JsonSerializer();
            var outputJson = parser.Serialize( obj );
        }

        [TestMethod]
        public void Test()
        {
            var obj = new ClassA()
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
            var json = parser.Serialize( obj );

            var objFromJson = parser.Deserialize<ClassA>( json );

            Assert.IsTrue( objFromJson.Simple1 == obj.Simple1 );
            Assert.IsTrue( objFromJson.Simple2 == obj.Simple2 );
            Assert.IsTrue( objFromJson.Simple3 == obj.Simple3 );
            Assert.IsTrue( objFromJson.ObjB.Simple1 == obj.ObjB.Simple1 );
            Assert.IsTrue( objFromJson.ObjB.Simple2 == obj.ObjB.Simple2 );
            Assert.IsTrue( objFromJson.ObjB.Simple3 == obj.ObjB.Simple3 );
        }
    }
}
