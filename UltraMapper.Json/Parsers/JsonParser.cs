using System;
using System.Collections.Generic;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    public class JsonParser : IParser
    {
#if NET5_0_OR_GREATER
        private readonly IParser Parser = new JsonParserUsingReadonlySpan();
#else
        private readonly IParser Parser = new JsonParserUsingSubstrings();
#endif

        public IParsedParam Parse( string text )
        {
            return this.Parser.Parse( text );
        }
    }
}
