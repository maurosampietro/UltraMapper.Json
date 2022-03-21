using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
#if NET5_0_OR_GREATER
    public class JsonParserUtf8ReadonlySpan : IParser
    {
        private enum ParseObjectState { PARAM_NAME, PARAM_VALUE }

        private const byte OBJECT_START_SYMBOL = (byte)'{';
        private const byte OBJECT_END_SYMBOL = (byte)'}';
        private const byte ARRAY_START_SYMBOL = (byte)'[';
        private const byte ARRAY_END_SYMBOL = (byte)']';
        private const byte PARAM_NAME_VALUE_DELIMITER = (byte)':';
        private const byte PARAMS_DELIMITER = (byte)',';
        private const byte QUOTE_SYMBOL = (byte)'"';
        private const byte ESCAPE_SYMBOL = (byte)'\\';

        private string _paramValue = String.Empty;
        private string _paramName = String.Empty;
        private string _itemValue = String.Empty;
        private string _quotedText = String.Empty;

        private byte _currentByte;

        public unsafe IParsedParam Parse( string str )
        {
            fixed( char* bText = str )
            {
                var bytes = FromString( str );
                return Parse( bytes );
            }
        }

        public IParsedParam Parse( ReadOnlySpan<byte> text )
        {
            for( int i = 0; true; i += 2 )
            {
                _currentByte = text[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

                switch( _currentByte )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i += 2;
                        return ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i += 2;
                        return ParseArray( text, ref i );
                    }

                    default:
                        throw new Exception( $"Unexpected symbol '{_currentByte}' at position {i}" );
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
        }

        private unsafe ReadOnlySpan<byte> FromString( string str )
        {
            fixed( char* bText = str )
                return new ReadOnlySpan<byte>( (byte*)bText, str.Length * 2 );
        }

        private const byte c1 = (byte)' ';
        private const byte c2 = (byte)'\x0009';
        private const byte c3 = (byte)'\x000d';
        private const byte c4 = (byte)'\x00a0';
        private const byte c5 = (byte)'\x0085';

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool IsWhiteSpace( byte c )
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
            return (c == c1) || (c >= c2 && c <= c3) || c == c4 || c == c5;
        }

        private ComplexParam ParseObject( ReadOnlySpan<byte> bytes,
            ref int i, ParseObjectState state )
        {
            var parsedParams = new List<IParsedParam>();

            for( ; true; i += 2 )
            {
                _currentByte = bytes[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

                int startIndex;
                switch( state )
                {
                    case ParseObjectState.PARAM_NAME:
                    {
                        for( ; state == ParseObjectState.PARAM_NAME; i += 2 )
                        {
                            _currentByte = bytes[ i ];

                            if( IsWhiteSpace( _currentByte ) )
                                continue;

                            switch( _currentByte )
                            {
                                case QUOTE_SYMBOL:
                                {
                                    i += 2;
                                    _paramName = ParseQuotation( bytes, ref i ).ToString();

                                    for( ; true; i++ )
                                    {
                                        _currentByte = bytes[ i ];

                                        if( IsWhiteSpace( _currentByte ) )
                                            continue;

                                        if( _currentByte == PARAM_NAME_VALUE_DELIMITER )
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
                                    throw new Exception( $"Unexpected symbol '{_currentByte}' at position {i}" );

                                default:
                                {
                                    startIndex = i;
                                    for( i++; true; i++ )
                                    {
                                        _currentByte = bytes[ i ];

                                        if( IsWhiteSpace( _currentByte ) )
                                            continue;

                                        if( _currentByte == PARAM_NAME_VALUE_DELIMITER )
                                            break;
                                    }

                                    _paramName = bytes[ startIndex..i ].ToString();

                                    state = ParseObjectState.PARAM_VALUE;
                                    i -= 2;

                                    break;
                                }
                            }
                        }

                        break;
                    }

                    case ParseObjectState.PARAM_VALUE:
                    {
                        for( ; state == ParseObjectState.PARAM_VALUE; i += 2 )
                        {
                            _currentByte = bytes[ i ];

                            if( IsWhiteSpace( _currentByte ) )
                                continue;

                            switch( _currentByte )
                            {
                                case QUOTE_SYMBOL:
                                {
                                    i += 2;

                                    var simpleParam = new SimpleParam()
                                    {
                                        Name = _paramName,
                                        Value = ParseQuotation( bytes, ref i ).ToString()
                                    };

                                    parsedParams.Add( simpleParam );

                                    _paramName = String.Empty;

                                    for( i += 2; true; i += 2 )
                                    {
                                        _currentByte = bytes[ i ];

                                        if( IsWhiteSpace( _currentByte ) )
                                            continue;

                                        if( _currentByte == OBJECT_END_SYMBOL )
                                        {
                                            return new ComplexParam()
                                            {
                                                Name = String.Empty,
                                                SubParams = parsedParams.ToArray()
                                            };
                                        }
                                        else if( _currentByte == PARAMS_DELIMITER )
                                            continue;
                                        else
                                        {
                                            state = ParseObjectState.PARAM_NAME;
                                            i -= 4;
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

                                    for( i += 2; true; i += 2 )
                                    {
                                        _currentByte = bytes[ i ];

                                        if( IsWhiteSpace( _currentByte ) )
                                            continue;

                                        if( _currentByte == OBJECT_END_SYMBOL )
                                        {
                                            return new ComplexParam()
                                            {
                                                Name = String.Empty,
                                                SubParams = parsedParams.ToArray()
                                            };
                                        }
                                        else if( _currentByte == PARAMS_DELIMITER )
                                            continue;
                                        else
                                        {
                                            state = ParseObjectState.PARAM_NAME;
                                            i -= 4;
                                            break;
                                        }
                                    }
                                    break;
                                }

                                case OBJECT_START_SYMBOL:
                                {
                                    i += 2;
                                    string paramName2 = _paramName;
                                    _paramName = String.Empty;

                                    var result = ParseObject( bytes, ref i, ParseObjectState.PARAM_NAME );
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
                                    i += 2;

                                    string paramname2 = _paramName;
                                    _paramName = String.Empty;

                                    var result = ParseArray( bytes, ref i );
                                    result.Name = paramname2;
                                    parsedParams.Add( result );
                                    break;
                                }

                                default:
                                {
                                    while( IsWhiteSpace( _currentByte ) )
                                        _currentByte = bytes[ i += 2 ];

                                    startIndex = i;
                                    for( ; true; i += 2 )
                                    {
                                        _currentByte = bytes[ i ];

                                        if( IsWhiteSpace( _currentByte ) )
                                            break;

                                        if( _currentByte == PARAMS_DELIMITER )
                                            break;
                                    }

                                    parsedParams.Add( new SimpleParam()
                                    {
                                        Name = _paramName,
                                        Value = bytes[ startIndex..i ].ToString()
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

        private ArrayParam ParseArray( ReadOnlySpan<byte> text, ref int i )
        {
            var items = new ArrayParam();

            for( ; true; i += 2 )
            {
                _currentByte = text[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

                switch( _currentByte )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i += 2;

                        var complexParam = ParseObject( text, ref i, ParseObjectState.PARAM_NAME );
                        items.Add( complexParam );

                        break;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i += 2;

                        var result = ParseArray( text, ref i );
                        items.Add( result );

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        i += 2;

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

                        i -= 2;

                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseQuotation( ReadOnlySpan<byte> text, ref int i )
        {
            bool escapeSymbols = false;
            bool unicodeSymbols = false;

            int startIndex = i;
            for( ; true; i += 2 )
            {
                _currentByte = text[ i ];

                switch( _currentByte )
                {
                    case ESCAPE_SYMBOL:
                    {
                        _currentByte = text[ i ];
                        i += 2;

                        escapeSymbols = true;

                        if( _currentByte == 'u' )
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
        private string ParseValue( ReadOnlySpan<byte> text, ref int i )
        {
            int startIndex = i;
            bool isParsingValue = false;

            for( ; i < text.Length; i += 2 )
            {
                _currentByte = text[ i ];

                if( IsWhiteSpace( _currentByte ) )
                {
                    if( isParsingValue )
                        return text[ startIndex..i ].ToString();

                    startIndex++;
                    continue;
                }

                switch( _currentByte )
                {
                    case PARAMS_DELIMITER:
                    case OBJECT_END_SYMBOL:
                    case ARRAY_END_SYMBOL:
                    {
                        return text[ startIndex..i ].ToString();
                    }

                    default:
                    {
                        if( !isParsingValue )
                            isParsingValue = true;

                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }
    }
#endif
}
