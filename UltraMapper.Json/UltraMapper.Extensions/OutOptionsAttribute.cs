using System;

namespace UltraMapper.Json.UltraMapper.Extensions
{
    internal class OutOptionsAttribute : Attribute
    {
        public bool IsIgnored { get; set; }
        public int Order { get; set; }
        public string Name { get; set; }
    }
}