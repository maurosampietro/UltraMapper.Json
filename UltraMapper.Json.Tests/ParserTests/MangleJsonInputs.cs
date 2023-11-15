using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.Json.Tests.ParserTests.JsonManglers;

namespace UltraMapper.Json.Tests.ParserTests
{
    [TestCategory( "Parser tests" )]
    [TestClass]
    public class DefaultTests : JsonParserTests
    {
        public DefaultTests()
            : base( new DoNothingMangler() ) { } //passing null is also ok :)
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class RemoveWhitespacesTests : JsonParserTests
    {
        public RemoveWhitespacesTests()
            : base( new RemoveWhitespacesMangler() )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddWhitespacesAtTheEndTests : JsonParserTests
    {
        public AddWhitespacesAtTheEndTests()
            : base( new AddWhiteSpacesAtTheEndMangler() )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddWhitespacesBeforeSpecialCharsTests : JsonParserTests
    {
        public AddWhitespacesBeforeSpecialCharsTests()
            : base( new AddWhitespacesMangler( addCharBefore: true, addCharAfter: false ) )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddWhitespacesAfterSpecialCharsTests : JsonParserTests
    {
        public AddWhitespacesAfterSpecialCharsTests()
            : base( new AddWhitespacesMangler( addCharBefore: false, addCharAfter: true ) )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddWhitespacesBeforeAndAfterSpecialCharsTests : JsonParserTests
    {
        public AddWhitespacesBeforeAndAfterSpecialCharsTests()
            : base( new AddWhitespacesMangler( true, true ) )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddCommasAndAddWhitespacesBeforeAndAfterSpecialCharsTests : JsonParserTests
    {
        public AddCommasAndAddWhitespacesBeforeAndAfterSpecialCharsTests()
            : base( new AddCommasMangler(), new AddWhitespacesMangler( true, true ) )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class AddCommasTests : JsonParserTests
    {
        public AddCommasTests()
            : base( new AddCommasMangler() )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class RemoveWhiteSpacesAndAddCommasTests : JsonParserTests
    {
        public RemoveWhiteSpacesAndAddCommasTests()
            : base( new RemoveWhitespacesMangler(), new AddCommasMangler() )
        {
        }
    }

    /// <summary>
    /// Let's test how relaxed we are with commas.
    /// JSON specs do not tolerate the absence of commas.
    /// </summary>
    [TestCategory( "Parser tests" )]
    [TestClass]
    public class RemoveCommasTests : JsonParserTests
    {
        public RemoveCommasTests()
            : base( new RemoveUnquotedCommasMangler() )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class ParamNameUpperCaseTests : JsonParserTests
    {
        public ParamNameUpperCaseTests()
            : base( new ParamNameCaseMangler( ParamNameCaseMangler.Cases.UPPER ) )
        {
        }
    }

    [TestCategory( "Parser tests" )]
    [TestClass]
    public class ParamNameLowerCaseTests : JsonParserTests
    {
        public ParamNameLowerCaseTests()
            : base( new ParamNameCaseMangler( ParamNameCaseMangler.Cases.LOWER ) )
        {
        }
    }
}
