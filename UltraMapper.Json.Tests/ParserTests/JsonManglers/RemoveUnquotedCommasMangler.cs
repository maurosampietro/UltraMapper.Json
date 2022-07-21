using System.Linq;
using System.Text;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class RemoveUnquotedCommasMangler : IJsonMangler
    {
        private readonly char[] _charsToLookFor = new[] { ',' };
        private readonly char _replacement = ' ';

        public string Mangle( string json )
        {
            var editedJson = new StringBuilder();

            bool isQuoted = false;
            bool isEscaped = false;

            foreach( var c in json )
            {
                if( c == '"' && !isEscaped )
                    isQuoted = !isQuoted;

                isEscaped = c == '\\';

                if( _charsToLookFor.Contains( c ) && !isQuoted )
                    editedJson.Append( _replacement );
                else editedJson.Append( c );
            }

            return editedJson.ToString();
        }
    }
}
