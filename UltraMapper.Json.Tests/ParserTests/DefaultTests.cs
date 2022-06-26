using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.Json.Tests.ParserTests.JsonManglers;

namespace UltraMapper.Json.Tests.ParserTests
{
    [TestCategory( "Parser tests" )]
    [TestClass]
    public class DefaultTests : JsonParserTests
    {
        public DefaultTests()
            : base( new DefaultMangler() ) { }
    }
}
