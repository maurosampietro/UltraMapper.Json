using System.Linq;
using UltraMapper.Json.Tests.ParserTests.Internals;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class RemoveWhitespacesMangler : IJsonMangler
    {
        public string Mangle( string json )
        {
            return new string( json.Where( c => !c.IsWhiteSpace() ).ToArray() );
        }
    }
}
