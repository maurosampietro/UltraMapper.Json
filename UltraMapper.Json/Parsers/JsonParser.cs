using UltraMapper.Json.Parsers;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    public class JsonParser : IParser
    {
//#if NET7_0_OR_GREATER
//        private readonly IParser Parser = new JsonParserUsingReadonlySpanAdapterP2AvxSupported();
//#else
#if NET5_0_OR_GREATER
        private readonly IParser Parser = new JsonParserUsingReadonlySpanAdapterP2();
#else
        private readonly IParser Parser = new JsonParserUsingSubstrings();
#endif
//#endif
        public IParsedParam Parse( string text )
        {
            return this.Parser.Parse( text );
        }
    }
}
