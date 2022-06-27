using System.Linq;
using System.Text;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class AddCommasMangler : AddCharsMangler
    {
        private static char[] _specialChars = new char[] { ',', '{', '}', '[', ']' };
        private const char _charToInsert = ',';

        public AddCommasMangler()
            : base( _charToInsert, _specialChars, addCharBefore: false, addCharAfter: true ) { }
    }
}
