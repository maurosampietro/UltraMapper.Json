using System.Linq;
using System.Text;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class AddCharsMangler : IJsonMangler
    {
        private readonly bool _addCharBefore;
        private readonly bool _addCharAfter;
        private readonly char _charToInsert;
        private readonly char[] _charsToLookFor;

        public AddCharsMangler( char charToInsert, char[] charsToLookFor, bool addCharBefore, bool addCharAfter )
        {
            _charToInsert = charToInsert;
            _charsToLookFor = charsToLookFor;
            _addCharBefore = addCharBefore;
            _addCharAfter = addCharAfter;
        }

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
                {
                    if( _addCharBefore )
                        editedJson.Append( _charToInsert );

                    editedJson.Append( c );

                    if( _addCharAfter )
                        editedJson.Append( _charToInsert );
                }
                else editedJson.Append( c );
            }

            return editedJson.ToString();
        }
    }
}
