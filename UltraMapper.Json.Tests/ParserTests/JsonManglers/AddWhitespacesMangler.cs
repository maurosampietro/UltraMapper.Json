using System.Linq;
using System.Text;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class AddWhitespacesMangler : AddCharsMangler
    {
        private static char[] _specialChars = new char[] { ',', ':', '{', '}', '[', ']' };
        private const char _charToInsert = '\t';

        public AddWhitespacesMangler( bool addCharBefore, bool addCharAfter )
            : base( _charToInsert, _specialChars, addCharBefore, addCharAfter ) { }
    }
}
