using System.Text.RegularExpressions;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class ParamNameCaseMangler : IJsonMangler
    {
        public enum Cases { UPPER, LOWER, MIXED }
        public readonly Cases @Case;

        public ParamNameCaseMangler( Cases @case )
        {
            this.Case = @case;
        }

        public string Mangle( string json )
        {
            var matches = Regex.Matches( json, @"\w+\s*:" );
            foreach( Match item in matches )
            {
                string token = json.Substring( item.Index, item.Length );
                
                switch( @Case )
                {
                    case Cases.UPPER: token = token.ToUpperInvariant(); break;
                    case Cases.LOWER: token = token.ToLowerInvariant(); break;
                }

                json = json.Replace( token, token.ToUpperInvariant() );
            }

            return json;
        }
    }
}
