using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UltraMapper.Parsing.Parameters2;

namespace UltraMapper.Json.Tests
{
    [TestClass]
    public class Sdfafasd
    {
        [TestMethod]
        public void Asdffsad()
        {
            string json = "[ 100 , 200, 300, 400, 500 ]";

            var parser = new JsonParserUsingReadonlySpanAdapterP2();
            var result = (ArrayParam2)parser.Parse( json );

            Assert.IsTrue( result.Simple.Count() == 5 );

            Assert.IsTrue( result.Simple[ 0 ].Value == "100" );
            Assert.IsTrue( result.Simple[ 1 ].Value == "200" );
            Assert.IsTrue( result.Simple[ 2 ].Value == "300" );
            Assert.IsTrue( result.Simple[ 3 ].Value == "400" );
            Assert.IsTrue( result.Simple[ 4 ].Value == "500" );
        }
    }
}
