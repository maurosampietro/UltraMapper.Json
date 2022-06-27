namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class DoNothingMangler : IJsonMangler
    {
        public string Mangle( string json )
        {
            return json;
        }
    }
}
