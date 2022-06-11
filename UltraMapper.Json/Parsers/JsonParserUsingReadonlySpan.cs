#if NET5_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    internal class JsonParserUsingReadonlySpan : IParser
    {
        private const char OBJECT_START_SYMBOL = '{';
        private const char OBJECT_END_SYMBOL = '}';
        private const char ARRAY_START_SYMBOL = '[';
        private const char ARRAY_END_SYMBOL = ']';
        private const char PARAM_NAME_VALUE_DELIMITER = ':';
        private const char PARAMS_DELIMITER = ',';
        private const char QUOTE_SYMBOL = '"';
        private const char ESCAPE_SYMBOL = '\\';

        private const int MIN_CAPACITY = 8;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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

            if( c == ' ' )
                return true;

            //if( (c & 8) != 8 )
            //    return false;

            return (c >= '\x0009' && c <= '\x000d') || c == '\x00a0' || c == '\x0085';

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

        public IParsedParam Parse( string text )
        {
            for( int i = 0; true; i++ )
            {
                switch( text[ i ] )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;
                        return ParseObject( text, ref i );
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;
                        return ParseArray( text, ref i );
                    }
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
        }

        private ComplexParam ParseObject( ReadOnlySpan<char> text, ref int i )
        {
            var parsedParams = new List<IParsedParam>( MIN_CAPACITY );
            ReadOnlySpan<char> _paramName = String.Empty;

        label:

            while( IsWhiteSpace( text[ i ] ) )
                i++;

            if( text[ i ] == OBJECT_END_SYMBOL )
            {
                return new ComplexParam()
                {
                    Name = _paramName.ToString(),
                    SubParams = parsedParams
                };
            }

            _paramName = ParseName( text, ref i );

            while( IsWhiteSpace( text[ i ] ) || text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                i++;

            int startIndex;
            for( ; true; i++ )
            {
                while( IsWhiteSpace( text[ i ] ) )
                    i++;

                switch( text[ i ] )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;
                        string paramName2 = _paramName.ToString();
                        _paramName = String.Empty;

                        var result = ParseObject( text, ref i );
                        parsedParams.Add( new ComplexParam()
                        {
                            Name = paramName2,
                            SubParams = result.SubParams
                        } );
                        break;
                    }

                    case OBJECT_END_SYMBOL:
                    {
                        return new ComplexParam()
                        {
                            Name = String.Empty,
                            SubParams = parsedParams
                        };
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;

                        string paramname2 = _paramName.ToString();
                        _paramName = String.Empty;

                        var result = ParseArray( text, ref i );
                        result.Name = paramname2;
                        parsedParams.Add( result );
                        break;
                    }

                    case PARAMS_DELIMITER:
                    {
                        i++;

                        while( IsWhiteSpace( text[ i ] ) )
                            i++;

                        _paramName = ParseName( text, ref i );
                        //_paramValue = String.Empty;
                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        var simpleParam = new SimpleParam()
                        {
                            Name = _paramName.ToString(),
                            Value = ParseQuotation( text, ref i )
                        };

                        parsedParams.Add( simpleParam );
                        break;
                    }

                    default:
                    {
                        startIndex = i;
                        for( ; true; i++ )
                        {
                            if( IsWhiteSpace( text[ i ] ) )
                                break;

                            if( text[ i ] == PARAMS_DELIMITER )
                                break;
                        }

                        parsedParams.Add( new SimpleParam()
                        {
                            Name = _paramName.ToString(),
                            Value = text[ startIndex..i ].ToString()
                        } );

                        i++;
                        goto label;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_END_SYMBOL}'" );
        }

        private ArrayParam ParseArray( ReadOnlySpan<char> text, ref int i )
        {
            var items = new ArrayParam();

            for( ; true; i++ )
            {
                while( IsWhiteSpace( text[ i ] ) )
                    i++;

                switch( text[ i ] )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;

                        var complexParam = ParseObject( text, ref i );
                        items.Add( complexParam );

                        break;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;

                        var result = ParseArray( text, ref i );
                        items.Add( result );

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        items.Add( new SimpleParam()
                        {
                            Value = ParseQuotation( text, ref i )
                        } );

                        break;
                    }

                    case PARAMS_DELIMITER:
                    {
                        break;
                    }

                    case ARRAY_END_SYMBOL:
                    {
                        return items;
                    }

                    default:
                    {
                        items.Add( new SimpleParam()
                        {
                            Value = ParseArrayValue( text, ref i )
                        } );

                        i--;

                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseQuotation( ReadOnlySpan<char> text, ref int i )
        {
            bool escapeSymbols = false;
            bool unicodeSymbols = false;

            //this method is call as soon as a " is found, so
            //we can immediately skip to the next 'i'            
            for( int startIndex = ++i; true; i++ )
            {
                switch( text[ i ] )
                {
                    case ESCAPE_SYMBOL:
                    {
                        ++i;

                        escapeSymbols = true;

                        if( text[ i ] == 'u' )
                            unicodeSymbols = true;

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        var quotation = text[ startIndex..i ].ToString();

                        if( escapeSymbols )
                        {
                            quotation = quotation
                                .Replace( @"\b", "\b" )
                                .Replace( @"\f", "\f" )
                                .Replace( @"\n", "\n" )
                                .Replace( @"\r", "\r" )
                                .Replace( @"\t", "\t" )
                                .Replace( @"\\", "\\" )
                                .Replace( @"\""", "\"" );

                            if( unicodeSymbols )
                            {
                                int unicodeCharIndex = quotation.IndexOf( @"\u" );
                                while( unicodeCharIndex > -1 )
                                {
                                    string unicodeLiteral = quotation.Substring( unicodeCharIndex, 6 );
                                    int code = Int32.Parse( unicodeLiteral.Substring( 2 ), System.Globalization.NumberStyles.HexNumber );
                                    string unicodeChar = Char.ConvertFromUtf32( code );
                                    quotation = quotation.Replace( unicodeLiteral, unicodeChar );

                                    unicodeCharIndex = quotation.IndexOf( @"\u" );
                                }
                            }
                        }

                        return quotation;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseArrayValue( ReadOnlySpan<char> text, ref int i )
        {
            //this is already done by the caller
            //while( IsWhiteSpace( text[ i ] ) )
            //    i++;

            int startIndex = i;
            for( ; true; i++ )
            {
                switch( text[ i ] )
                {
                    case PARAMS_DELIMITER:
                    case ARRAY_END_SYMBOL:
                        return text[ startIndex..i ].ToString();
                }

                //most expensive check = last check
                if( IsWhiteSpace( text[ i ] ) )
                    return text[ startIndex..i ].ToString();
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseName( ReadOnlySpan<char> text, ref int i )
        {
            //this is already done by the caller
            //while( IsWhiteSpace( text[ i ] ) )
            //    i++;

            ReadOnlySpan<char> _paramName;

            if( text[ i ] == QUOTE_SYMBOL )
            {
                _paramName = ParseQuotation( text, ref i );

                for( i++; true; i++ )
                {
                    if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                        break;
                }

                return _paramName.ToString();
            }
            else
            {
                int startIndex = i;
                for( ; true; i++ )
                {
                    if( IsWhiteSpace( text[ i ] ) )
                    {
                        _paramName = text[ startIndex..i ];

                        for( i++; true; i++ )
                        {
                            if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                                break;
                        }

                        return _paramName.ToString();
                    }
                    
                    if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                        return text[ startIndex..i ].ToString();
                }
            }
        }
    }
}
#endif
