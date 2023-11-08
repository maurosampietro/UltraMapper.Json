using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    public class JsonParser : IParser
    {
#if NET5_0_OR_GREATER
        private readonly IParser Parser = new JsonParserUsingReadonlySpanAdapter();
#else
        private readonly IParser Parser = new JsonParserUsingSubstrings();
#endif

        public IParsedParam Parse( string text )
        {
            return this.Parser.Parse( text );
        }
    }
}
