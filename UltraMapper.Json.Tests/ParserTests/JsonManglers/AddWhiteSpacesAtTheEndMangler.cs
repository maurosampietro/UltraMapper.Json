namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class AddWhiteSpacesAtTheEndMangler : IJsonMangler
    {
        public string Mangle( string json )
        {
            return json += new string( ' ', 31 );
        }
    }
}
