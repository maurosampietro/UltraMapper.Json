namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class DefaultMangler : IJsonMangler
    {
        public string Mangle( string json )
        {
            return json;
        }
    }
}
