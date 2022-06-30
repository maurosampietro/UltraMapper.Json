using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace UltraMapper.Json
{
    internal static class StringExtensions
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsWhiteSpace( this char c )
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
            return (c == ' ') || (c >= '\x0009' && c <= '\x000d') || c == '\x00a0' || c == '\x0085';

            ////the above code is faster than:
            //switch( c )
            //{
            //    case ' ': return true;
            //    case '\x0009': return true;
            //    case '\x000a': return true;
            //    case '\x000b': return true;
            //    case '\x000c': return true;
            //    case '\x000d': return true;
            //    case '\x00a0': return true;
            //    case '\x0085': return true;
            //    default: return false;
            //}
        }
    }
}
