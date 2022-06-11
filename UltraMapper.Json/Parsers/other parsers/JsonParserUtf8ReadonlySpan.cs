//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Text;
//using UltraMapper.Parsing;

//namespace UltraMapper.Json
//{
//#if NET5_0_OR_GREATER
//    public class JsonParserUtf8ReadonlySpan : IParser
//    {
//        private enum ParseObjectState { PARAM_NAME, PARAM_VALUE }

//        private const byte OBJECT_START_SYMBOL = (byte)'{';
//        private const byte OBJECT_END_SYMBOL = (byte)'}';
//        private const byte ARRAY_START_SYMBOL = (byte)'[';
//        private const byte ARRAY_END_SYMBOL = (byte)']';
//        private const byte PARAM_NAME_VALUE_DELIMITER = (byte)':';
//        private const byte PARAMS_DELIMITER = (byte)',';
//        private const byte QUOTE_SYMBOL = (byte)'"';
//        private const byte ESCAPE_SYMBOL = (byte)'\\';

//        public unsafe IParsedParam Parse( string str )
//        {
//            var bytes = Encoding.UTF8.GetBytes( str, 0, str.Length );
//            return Parse( bytes, str );
//        }

//        public IParsedParam Parse( byte[] bytes, string chars )
//        {
//            for( int i = 0; true; i++ )
//            {
//                if( IsWhiteSpace( bytes[ i ] ) )
//                    continue;

//                switch( bytes[ i ] )
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        i++;
//                        return ParseObject( bytes, chars, ref i );
//                    }

//                    case ARRAY_START_SYMBOL:
//                    {
//                        i++;
//                        return ParseArray( bytes, chars, ref i );
//                    }

//                    default:
//                        throw new Exception( $"Unexpected symbol '{bytes[ i ]}' at position {i}" );
//                }
//            }

//            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
//        }

//        private unsafe ReadOnlySpan<byte> ReadonlySpanByteFromString( string str )
//        {
//            fixed( char* bText = str )
//                return new ReadOnlySpan<byte>( (byte*)bText, str.Length * 2 );
//        }

//        private unsafe ReadOnlySpan<char> ReadonlySpanCharFromString( string str )
//        {
//            fixed( char* bText = str )
//                return new ReadOnlySpan<char>( bText, str.Length );
//        }

//        private const byte c1 = (byte)' ';
//        private const byte c2 = (byte)'\x0009';
//        private const byte c3 = (byte)'\x000d';
//        private const byte c4 = (byte)'\x00a0';
//        private const byte c5 = (byte)'\x0085';

//        public const int MIN_CAPACITY = 8;

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        public static bool IsWhiteSpace( byte c )
//        {
//            // There are characters which belong to UnicodeCategory.Control but are considered as white spaces.
//            // We use code point comparisons for these characters here as a temporary fix.

//            // U+0009 = <control> HORIZONTAL TAB
//            // U+000a = <control> LINE FEED
//            // U+000b = <control> VERTICAL TAB
//            // U+000c = <contorl> FORM FEED
//            // U+000d = <control> CARRIAGE RETURN
//            // U+0085 = <control> NEXT LINE
//            // U+00a0 = NO-BREAK SPACE
//            return (c == c1) || (c >= c2 && c <= c3) || c == c4 || c == c5;
//        }

//        private ComplexParam ParseObject( byte[] bytes, string text, ref int i )
//        {
//            var parsedParams = new List<IParsedParam>( MIN_CAPACITY );
//            string _paramName = String.Empty;

//        label:

//            while( IsWhiteSpace( bytes[ i ] ) )
//                i++;

//            if( bytes[ i ] == OBJECT_END_SYMBOL )
//            {
//                return new ComplexParam()
//                {
//                    Name = _paramName.ToString(),
//                    SubParams = parsedParams
//                };
//            }

//            _paramName = ParseName( bytes, text, ref i );

//            while( IsWhiteSpace( bytes[ i ] ) || bytes[ i ] == PARAM_NAME_VALUE_DELIMITER )
//                i++;

//            int startIndex;
//            for( ; true; i++ )
//            {
//                while( IsWhiteSpace( bytes[ i ] ) )
//                    i++;

//                switch( bytes[ i ] )
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        i++;
//                        string paramName2 = _paramName.ToString();
//                        _paramName = String.Empty;

//                        var result = ParseObject( bytes, text, ref i );
//                        parsedParams.Add( new ComplexParam()
//                        {
//                            Name = paramName2,
//                            SubParams = result.SubParams
//                        } );
//                        break;
//                    }

//                    case OBJECT_END_SYMBOL:
//                    {
//                        return new ComplexParam()
//                        {
//                            Name = String.Empty,
//                            SubParams = parsedParams
//                        };
//                    }

//                    case ARRAY_START_SYMBOL:
//                    {
//                        i++;

//                        string paramname2 = _paramName.ToString();
//                        _paramName = String.Empty;

//                        var result = ParseArray( bytes, text, ref i );
//                        result.Name = paramname2;
//                        parsedParams.Add( result );
//                        break;
//                    }

//                    case PARAMS_DELIMITER:
//                    {
//                        i++;
//                        _paramName = ParseName( bytes, text, ref i );
//                        //_paramValue = String.Empty;
//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        i++;

//                        var simpleParam = new SimpleParam()
//                        {
//                            Name = _paramName.ToString(),
//                            Value = ParseQuotation( bytes, text, ref i )
//                        };

//                        parsedParams.Add( simpleParam );
//                        break;
//                    }

//                    default:
//                    {
//                        while( IsWhiteSpace( bytes[ i ] ) )
//                            i++;

//                        startIndex = i;
//                        for( ; true; i++ )
//                        {
//                            if( IsWhiteSpace( bytes[ i ] ) )
//                                break;

//                            if( bytes[ i ] == PARAMS_DELIMITER )
//                                break;
//                        }

//                        parsedParams.Add( new SimpleParam()
//                        {
//                            Name = _paramName.ToString(),
//                            Value = text[ startIndex..i ].ToString()
//                        } );

//                        i++;
//                        goto label;
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{OBJECT_END_SYMBOL}'" );
//        }

//        private ArrayParam ParseArray( byte[] bytes, string text, ref int i )
//        {
//            var items = new ArrayParam();

//            for( ; true; i++ )
//            {
//                while( IsWhiteSpace( bytes[ i ] ) )
//                    i++;

//                switch( bytes[ i ] )
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        i++;

//                        var complexParam = ParseObject( bytes, text, ref i );
//                        items.Add( complexParam );

//                        break;
//                    }

//                    case ARRAY_START_SYMBOL:
//                    {
//                        i++;

//                        var result = ParseArray( bytes, text, ref i );
//                        items.Add( result );

//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        i++;

//                        items.Add( new SimpleParam()
//                        {
//                            Value = ParseQuotation( bytes, text, ref i )
//                        } );

//                        break;
//                    }

//                    case PARAMS_DELIMITER:
//                    {
//                        break;
//                    }

//                    case ARRAY_END_SYMBOL:
//                    {
//                        return items;
//                    }

//                    default:
//                    {
//                        items.Add( new SimpleParam()
//                        {
//                            Value = ParseValue( bytes, text, ref i )
//                        } );

//                        i--;

//                        break;
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
//        }

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private string ParseQuotation( byte[] bytes, string text, ref int i )
//        {
//            bool escapeSymbols = false;
//            bool unicodeSymbols = false;

//            int startIndex = i;
//            for( ; true; i++ )
//            {
//                switch( bytes[ i ] )
//                {
//                    case ESCAPE_SYMBOL:
//                    {
//                        ++i;

//                        escapeSymbols = true;

//                        if( bytes[ i ] == 'u' )
//                            unicodeSymbols = true;

//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        var quotation = text[ startIndex..i ].ToString();

//                        if( escapeSymbols )
//                        {
//                            quotation = quotation
//                                .Replace( @"\b", "\b" )
//                                .Replace( @"\f", "\f" )
//                                .Replace( @"\n", "\n" )
//                                .Replace( @"\r", "\r" )
//                                .Replace( @"\t", "\t" )
//                                .Replace( @"\\", "\\" )
//                                .Replace( @"\""", "\"" );

//                            if( unicodeSymbols )
//                            {
//                                int unicodeCharIndex = quotation.IndexOf( @"\u" );
//                                while( unicodeCharIndex > -1 )
//                                {
//                                    string unicodeLiteral = quotation.Substring( unicodeCharIndex, 6 );
//                                    int code = Int32.Parse( unicodeLiteral.Substring( 2 ), System.Globalization.NumberStyles.HexNumber );
//                                    string unicodeChar = Char.ConvertFromUtf32( code );
//                                    quotation = quotation.Replace( unicodeLiteral, unicodeChar );

//                                    unicodeCharIndex = quotation.IndexOf( @"\u" );
//                                }
//                            }
//                        }

//                        return quotation;
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
//        }

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private string ParseValue( byte[] bytes, string text, ref int i )
//        {
//            int startIndex = i;

//            while( IsWhiteSpace( bytes[ i ] ) )
//            {
//                startIndex++;
//                i++;
//            }

//            for( ; true; i++ )
//            {
//                if( IsWhiteSpace( bytes[ i ] ) )
//                    return text[ startIndex..i ].ToString();

//                switch( bytes[ i ] )
//                {
//                    case PARAMS_DELIMITER:
//                    case OBJECT_END_SYMBOL:
//                    case ARRAY_END_SYMBOL:
//                    {
//                        return text[ startIndex..i ].ToString();
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
//        }

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private string ParseName( byte[] bytes, string text, ref int i )
//        {
//            while( IsWhiteSpace( bytes[ i ] ) )
//                i++;

//            string _paramName;

//            switch( bytes[ i ] )
//            {
//                case QUOTE_SYMBOL:

//                    i++;
//                    _paramName = ParseQuotation( bytes, text, ref i );

//                    for( i++; true; i++ )
//                    {
//                        if( bytes[ i ] == PARAM_NAME_VALUE_DELIMITER )
//                            break;
//                    }

//                    return _paramName.ToString();

//                default:
//                {
//                    int startIndex = i;
//                    for( ; true; i++ )
//                    {
//                        if( IsWhiteSpace( bytes[ i ] ) )
//                        {
//                            _paramName = text[ startIndex..i ].ToString();

//                            for( i++; true; i++ )
//                            {
//                                if( bytes[ i ] == PARAM_NAME_VALUE_DELIMITER )
//                                    break;
//                            }

//                            return _paramName.ToString();
//                        }

//                        if( bytes[ i ] == PARAM_NAME_VALUE_DELIMITER )
//                            return text[ startIndex..i ].ToString();
//                    }
//                }
//            }
//        }
//    }
//#endif
//}
