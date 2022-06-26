﻿using System.Linq;

namespace UltraMapper.Json.Tests.ParserTests.JsonManglers
{
    public class RemoveWhitespacesMangler : IJsonMangler
    {
        public static bool IsWhiteSpace( char c )
        {
            // There are characters which belong to UnicodeCategory.Control but are considered as white spaces.
            // We use code point comparisons for these characters here as a temporary fix.

            // U+0009 = <control> HORIZONTAL TAB
            // U+000a = <control> LINE FEED
            // U+000b = <control> VERTICAL TAB
            // U+000c = <contorl> FORM FEED
            // U+000d = <control> CARRIAGE RETURN
            // U+0085 = <control> NEXT LINE
            // U+00a0 = NO-BREAK SPACE
            return c == ' ' || c >= '\x0009' && c <= '\x000d' || c == '\x00a0' || c == '\x0085';
        }

        public string Mangle( string json )
        {
            return new string( json.Where( c => !IsWhiteSpace( c ) ).ToArray() );
        }
    }
}
