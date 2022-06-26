using System.Linq;
using System.Text;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class AddWhitespacesMangler : IJsonMangler
    {
        private static char[] _specialChars = new char[] { ',', ':', '{', '}', '[', ']' };
        private readonly bool _addCharBefore;
        private readonly bool _addCharAfter;

        public AddWhitespacesMangler( bool addCharBefore, bool addCharAfter )
        {
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
                if( c == '"' && !isEscaped ) isQuoted = !isQuoted;
                
                isEscaped = c == '\\';

                if( _specialChars.Contains( c ) && !isQuoted )
                {
                    if( _addCharBefore )
                        editedJson.Append( '\t' );

                    editedJson.Append( c );

                    if( _addCharAfter )
                        editedJson.Append( '\t' ); 
                }
                else editedJson.Append( c );
            }

            return editedJson.ToString();
        }
    }
}
