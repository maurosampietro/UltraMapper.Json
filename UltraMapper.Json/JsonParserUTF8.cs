using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    public class ByteBuilder
    {
        private byte[] _data = new byte[ 64 ];
        private int index = 0;

        public void Append( byte b )
        {
            _data[ index ] = b;
            index++;
        }

        public int Length => index;
        public void Clear() { index = 0; }

        public override string ToString()
        {
            return Encoding.UTF8.GetString( _data, 0, index );
        }
    }

    public class JsonParserUtf8 : IParser
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

        private readonly ByteBuilder _paramValue = new ByteBuilder();
        private readonly ByteBuilder _paramName = new ByteBuilder();
        private readonly ByteBuilder _itemValue = new ByteBuilder();
        private readonly ByteBuilder _quotedText = new ByteBuilder();

        private byte _currentByte;

        public unsafe IParsedParam Parse( string text )
        {
            fixed( char* bText = text )
            {
                var b = (byte*)bText;
                return Parse( b, text );
            }
        }

        public unsafe IParsedParam Parse( byte* bytes, string text )
        {
            for( int i = 0; i < text.Length * 2; i += 2 )
            {
                _currentByte = bytes[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

                switch( _currentByte )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i += 2;
                        return ParseObject( bytes, text, ref i, ParseObjectState.PARAM_NAME );
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i += 2;
                        return ParseArray( bytes, text, ref i );
                    }

                    default:
                        throw new Exception( $"Unexpected symbol '{_currentByte}' at position {i}" );
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
        }

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
            return (c == (byte)' ') || (c >= (byte)'\x0009' && c <= (byte)'\x000d') || c == (byte)'\x00a0' || c == (byte)'\x0085';
        }

        private unsafe ComplexParam ParseObject( byte* bytes, string text,
            ref int i, ParseObjectState state )
        {
            var parsedParams = new List<IParsedParam>();
            bool isAdded = false;

            for( ; i < text.Length * 2; i += 2 )
            {
                _currentByte = bytes[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

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
                                    ParseQuotation( bytes, text, ref i, _paramName );
                                    state = ParseObjectState.PARAM_VALUE;
                                    i -= 2;
                                    break;
                                }

                                case PARAM_NAME_VALUE_DELIMITER:
                                {
                                    state = ParseObjectState.PARAM_VALUE;
                                    i -= 2;
                                    break;
                                }

                                case OBJECT_END_SYMBOL:
                                {
                                    return new ComplexParam()
                                    {
                                        Name = _paramName.ToString(),
                                        SubParams = null
                                    };
                                }

                                case PARAMS_DELIMITER:
                                case OBJECT_START_SYMBOL:
                                case ARRAY_START_SYMBOL:
                                    throw new Exception( $"Unexpected symbol '{_currentByte}' at position {i}" );

                                default:
                                {
                                    _paramName.Append( _currentByte );
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
                                    ParseQuotation( bytes, text, ref i, _paramValue );

                                    var simpleParam = new SimpleParam()
                                    {
                                        Name = _paramName.ToString(),
                                        Value = _paramValue.ToString()
                                    };

                                    _paramName.Clear();
                                    _paramValue.Clear();

                                    parsedParams.Add( simpleParam );

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
                                            isAdded = false;
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
                                            Name = _paramName.ToString(),
                                            Value = _paramValue.ToString()
                                        };

                                        _paramName.Clear();
                                        _paramValue.Clear();

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
                                            isAdded = false;
                                            state = ParseObjectState.PARAM_NAME;
                                            i -= 4;
                                            break;
                                        }
                                    }
                                    break;
                                }

                                case PARAM_NAME_VALUE_DELIMITER:
                                {
                                    continue;
                                }

                                case OBJECT_START_SYMBOL:
                                {
                                    i += 2;
                                    string paramName2 = _paramName.ToString();
                                    _paramName.Clear();

                                    var result = ParseObject( bytes, text, ref i, ParseObjectState.PARAM_NAME );
                                    parsedParams.Add( new ComplexParam()
                                    {
                                        Name = paramName2,
                                        SubParams = result.SubParams
                                    } );

                                    isAdded = true;
                                    break;
                                }

                                case OBJECT_END_SYMBOL:
                                {
                                    if( !isAdded )
                                    {
                                        parsedParams.Add( new SimpleParam()
                                        {
                                            Name = _paramName.ToString(),
                                            Value = _paramValue.ToString()
                                        } );
                                    }

                                    return new ComplexParam()
                                    {
                                        Name = String.Empty,
                                        SubParams = parsedParams.ToArray()
                                    };
                                }

                                case ARRAY_START_SYMBOL:
                                {
                                    i += 2;

                                    string paramname2 = _paramName.ToString();
                                    _paramName.Clear();

                                    var result = ParseArray( bytes, text, ref i );
                                    result.Name = paramname2;
                                    parsedParams.Add( result );

                                    isAdded = true;
                                    break;
                                }

                                default:
                                {
                                    _paramValue.Append( _currentByte );
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

        private unsafe ArrayParam ParseArray( byte* bytes, string text, ref int i )
        {
            var items = new ArrayParam();

            for( ; i < text.Length * 2; i += 2 )
            {
                _currentByte = bytes[ i ];

                if( IsWhiteSpace( _currentByte ) )
                    continue;

                switch( _currentByte )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i += 2;

                        var obj = ParseObject( bytes, text, ref i, ParseObjectState.PARAM_NAME );
                        items.Add( obj );

                        break;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i += 2;

                        var result = ParseArray( bytes, text, ref i );
                        items.Add( result );

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        i += 2;

                        ParseQuotation( bytes, text, ref i, _quotedText );
                        items.Add( new SimpleParam() { Value = _quotedText.ToString() } );

                        break;
                    }

                    case PARAMS_DELIMITER:
                    {
                        if( _itemValue.Length > 0 )
                        {
                            items.Add( new SimpleParam() { Value = _itemValue.ToString() } );
                            _itemValue.Clear();
                        }

                        break;
                    }

                    case ARRAY_END_SYMBOL:
                    {
                        if( _itemValue.Length > 0 )
                        {
                            items.Add( new SimpleParam() { Value = _itemValue.ToString() } );
                            _itemValue.Clear();
                        }

                        return items;
                    }

                    default:
                    {
                        _itemValue.Append( _currentByte );
                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        private unsafe void ParseQuotation( byte* bytes, string text, ref int i, ByteBuilder _quotedText )
        {
            _quotedText.Clear();

            for( ; i < text.Length * 2; i += 2 )
            {
                _currentByte = bytes[ i ];

                switch( _currentByte )
                {
                    case ESCAPE_SYMBOL:
                    {
                        i += 2;
                        _currentByte = bytes[ i ];

                        switch( _currentByte )
                        {
                            case (byte)'b': _quotedText.Append( (byte)'\b' ); break;
                            case (byte)'f': _quotedText.Append( (byte)'\f' ); break;
                            case (byte)'n': _quotedText.Append( (byte)'\n' ); break;
                            case (byte)'r': _quotedText.Append( (byte)'\r' ); break;
                            case (byte)'t': _quotedText.Append( (byte)'\t' ); break;
                            case (byte)'u':
                            {
                                i += 2;

                                string unicodeLiteral = String.Empty;
                                for( int k = 0; k < 4; k++ )
                                    unicodeLiteral += (char)text[ i + k ];

                                i += 3;

                                int code = Int32.Parse( unicodeLiteral, System.Globalization.NumberStyles.HexNumber );
                                string unicodeChar = Char.ConvertFromUtf32( code );
                                //_quotedText.Append( unicodeChar );

                                break;
                            }

                            default:
                            {
                                _quotedText.Append( _currentByte );
                                break;
                            }
                        }

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        return;
                    }

                    default:
                    {
                        _quotedText.Append( _currentByte );
                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }
    }
}
