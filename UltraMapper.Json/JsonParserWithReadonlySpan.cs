using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
#if NET5_0_OR_GREATER

    public class JsonParserWithReadonlySpan : IParser
    {
        private enum ParseObjectState { PARAM_NAME, PARAM_VALUE }

        private const char OBJECT_START_SYMBOL = '{';
        private const char OBJECT_END_SYMBOL = '}';
        private const char ARRAY_START_SYMBOL = '[';
        private const char ARRAY_END_SYMBOL = ']';
        private const char PARAM_NAME_VALUE_DELIMITER = ':';
        private const char PARAMS_DELIMITER = ',';
        private const char QUOTE_SYMBOL = '"';
        private const char ESCAPE_SYMBOL = '\\';

        private string _paramValue = String.Empty;
        private string _paramName = String.Empty;

        private char _currentChar;

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
            return (c == ' ') || (c >= '\x0009' && c <= '\x000d') || c == '\x00a0' || c == '\x0085';
        }

        public IParsedParam Parse( string text )
        {
            if( String.IsNullOrWhiteSpace( text ) )
                return null;
                
            for( int i = 0; true; i++ )
            {
                _currentChar = text[ i ];

                if( IsWhiteSpace( _currentChar ) )
                    continue;

                switch( _currentChar )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;
                        return ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;
                        return ParseArray( text, ref i );
                    }

                    default:
                        throw new Exception( $"Unexpected symbol '{_currentChar}' at position {i}" );
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
        }

        private ComplexParam ParseObject( ReadOnlySpan<char> text,
            ref int i, ParseObjectState state )
        {
            var parsedParams = new List<IParsedParam>();

            for( ; true; i++ )
            {
                _currentChar = text[ i ];

                if( IsWhiteSpace( _currentChar ) )
                    continue;

                int startIndex;
                switch( state )
                {
                    case ParseObjectState.PARAM_NAME:
                    {
                        for( ; state == ParseObjectState.PARAM_NAME; i++ )
                        {
                            _currentChar = text[ i ];

                            if( IsWhiteSpace( _currentChar ) )
                                continue;

                            switch( _currentChar )
                            {
                                case QUOTE_SYMBOL:
                                {
                                    i++;
                                    _paramName = ParseQuotation( text, ref i );

                                    for( ; true; i++ )
                                    {
                                        _currentChar = text[ i ];

                                        if( IsWhiteSpace( _currentChar ) )
                                            continue;

                                        if( _currentChar == PARAM_NAME_VALUE_DELIMITER )
                                            break;
                                    }

                                    state = ParseObjectState.PARAM_VALUE;
                                    break;
                                }

                                case OBJECT_END_SYMBOL:
                                {
                                    return new ComplexParam()
                                    {
                                        Name = _paramName,
                                        SubParams = parsedParams.ToArray()
                                    };
                                }

                                case PARAMS_DELIMITER:
                                case OBJECT_START_SYMBOL:
                                case ARRAY_START_SYMBOL:
                                    throw new Exception( $"Unexpected symbol '{_currentChar}' at position {i}" );

                                default:
                                {
                                    startIndex = i;
                                    for( i++; true; i++ )
                                    {
                                        _currentChar = text[ i ];

                                        if( IsWhiteSpace( _currentChar ) )
                                            continue;

                                        if( _currentChar == PARAM_NAME_VALUE_DELIMITER )
                                            break;
                                    }

                                    _paramName = text[ startIndex..i ].ToString();

                                    state = ParseObjectState.PARAM_VALUE;
                                    i--;

                                    break;
                                }
                            }
                        }

                        break;
                    }

                    case ParseObjectState.PARAM_VALUE:
                    {
                        for( ; state == ParseObjectState.PARAM_VALUE; i++ )
                        {
                            _currentChar = text[ i ];

                            if( IsWhiteSpace( _currentChar ) )
                                continue;

                            switch( _currentChar )
                            {
                                case QUOTE_SYMBOL:
                                {
                                    i++;

                                    var simpleParam = new SimpleParam()
                                    {
                                        Name = _paramName,
                                        Value = ParseQuotation( text, ref i )
                                    };

                                    parsedParams.Add( simpleParam );

                                    _paramName = String.Empty;

                                    for( i++; true; i++ )
                                    {
                                        _currentChar = text[ i ];

                                        if( IsWhiteSpace( _currentChar ) )
                                            continue;

                                        if( _currentChar == OBJECT_END_SYMBOL )
                                        {
                                            return new ComplexParam()
                                            {
                                                Name = String.Empty,
                                                SubParams = parsedParams.ToArray()
                                            };
                                        }
                                        else if( _currentChar == PARAMS_DELIMITER )
                                            continue;
                                        else
                                        {
                                            state = ParseObjectState.PARAM_NAME;
                                            i -= 2;
                                            break;
                                        }
                                    }

                                    break;
                                }

                                case PARAMS_DELIMITER:
                                {
                                    if( _paramName.Length > 0 )
                                    {
                                        var simpleParam = new SimpleParam()
                                        {
                                            Name = _paramName,
                                            Value = _paramValue
                                        };

                                        _paramName = String.Empty;
                                        _paramValue = String.Empty;

                                        parsedParams.Add( simpleParam );
                                    }

                                    for( i++; true; i++ )
                                    {
                                        _currentChar = text[ i ];

                                        if( IsWhiteSpace( _currentChar ) )
                                            continue;

                                        if( _currentChar == OBJECT_END_SYMBOL )
                                        {
                                            return new ComplexParam()
                                            {
                                                Name = String.Empty,
                                                SubParams = parsedParams.ToArray()
                                            };
                                        }
                                        else if( _currentChar == PARAMS_DELIMITER )
                                            continue;
                                        else
                                        {
                                            state = ParseObjectState.PARAM_NAME;
                                            i -= 2;
                                            break;
                                        }
                                    }
                                    break;
                                }

                                case OBJECT_START_SYMBOL:
                                {
                                    i++;
                                    string paramName2 = _paramName;
                                    _paramName = String.Empty;

                                    var result = ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
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
                                        SubParams = parsedParams.ToArray()
                                    };
                                }

                                case ARRAY_START_SYMBOL:
                                {
                                    i++;

                                    string paramname2 = _paramName;
                                    _paramName = String.Empty;

                                    var result = ParseArray( text, ref i );
                                    result.Name = paramname2;
                                    parsedParams.Add( result );
                                    break;
                                }

                                default:
                                {
                                    while( IsWhiteSpace( _currentChar ) )
                                        _currentChar = text[ i++ ];

                                    startIndex = i;
                                    for( ; true; i++ )
                                    {
                                        _currentChar = text[ i ];

                                        if( IsWhiteSpace( _currentChar ) )
                                            break;

                                        if( _currentChar == PARAMS_DELIMITER )
                                            break;
                                    }

                                    parsedParams.Add( new SimpleParam()
                                    {
                                        Name = _paramName,
                                        Value = text[ startIndex..i ].ToString()
                                    } );

                                    state = ParseObjectState.PARAM_NAME;
                                    break;
                                }
                            }
                        }

                        break;
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
                _currentChar = text[ i ];

                if( IsWhiteSpace( _currentChar ) )
                    continue;

                switch( _currentChar )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;

                        var complexParam = ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
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
                        i++;

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
                            Value = ParseValue( text, ref i )
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
          
            int startIndex = i;
            for( ; true; i++ )
            {
                _currentChar = text[ i ];

                switch( _currentChar )
                {
                    case ESCAPE_SYMBOL:
                    {
                        _currentChar = text[ ++i ];

                        escapeSymbols = true;

                        if( _currentChar == 'u' )
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
        private string ParseValue( ReadOnlySpan<char> text, ref int i )
        {
            int startIndex = i;
            bool isParsingValue = false;

            for( ; i < text.Length; i++ )
            {
                _currentChar = text[ i ];

                if( IsWhiteSpace( _currentChar ) )
                {
                    startIndex++;
                    continue;
                }

                for( ; i < text.Length; i++ )
                {
                    _currentChar = text[ i ];
                    if( IsWhiteSpace( _currentChar ) )
                        return text[ startIndex..i ].ToString();

                    switch( _currentChar )
                    {
                        case PARAMS_DELIMITER:
                        case OBJECT_END_SYMBOL:
                        case ARRAY_END_SYMBOL:
                        {
                            return text[ startIndex..i ].ToString();
                        }
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }
    }
#endif
}
