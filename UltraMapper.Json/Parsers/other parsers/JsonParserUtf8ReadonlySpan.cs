//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System.Text;
//using UltraMapper.Parsing;

//namespace UltraMapper.Json
//{
//#if NET5_0_OR_GREATER
//    public class JsonParserUtf8ReadonlySpanAdapter : IParser
//    {
//        public IParsedParam Parse( string text )
//        {
//            return new JsonParserUtf8ReadonlySpan( text ).Parse();
//        }
//    }

//    /// <summary>
//    /// La maggior parte del tempo viene perso facendo .ToString() sui parametri.
//    /// </summary>
//    /// 
//    public ref struct JsonParserUtf8ReadonlySpan
//    {
//        private readonly ReadOnlySpan<byte> _bytes;
//        private readonly byte[] _bytesRaw;
//        private int _idx = 0;

//        private const byte OBJECT_START_SYMBOL = (byte)'{';
//        private const byte OBJECT_END_SYMBOL = (byte)'}';
//        private const byte ARRAY_START_SYMBOL = (byte)'[';
//        private const byte ARRAY_END_SYMBOL = (byte)']';
//        private const byte PARAM_NAME_VALUE_DELIMITER = (byte)':';
//        private const byte PARAMS_DELIMITER = (byte)',';
//        private const byte QUOTE_SYMBOL = (byte)'"';
//        private const byte ESCAPE_SYMBOL = (byte)'\\';

//        public JsonParserUtf8ReadonlySpan( string str )
//        {
//            _bytesRaw = Encoding.UTF8.GetBytes( str );
//            _bytes = _bytesRaw;
//        }

//        public IParsedParam Parse()
//        {
//            for(; true; _idx++)
//            {
//                if(IsWhiteSpace( _bytes[ _idx ] ))
//                    continue;

//                switch(_bytes[ _idx ])
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        _idx++;
//                        return ParseObject();
//                    }

//                    case ARRAY_START_SYMBOL:
//                    {
//                        _idx++;
//                        return ParseArray();
//                    }

//                    default:
//                        throw new Exception( $"Unexpected symbol '{_bytes[ _idx ]}' at position {_idx}" );
//                }
//            }

//            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
//        }

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
//            return c is 32 or 9 or 10 or 13;
//        }

//        private ParseJsonComplexParam ParseObject()
//        {
//            var cp = new ParseJsonComplexParam( _bytesRaw );

//            var parsedParams = new List<ParseJsonComplexParam>();
//            (int startIndex, int endIndex) _paramName;

//        label:

//            while(IsWhiteSpace( _bytes[ _idx ] ))
//                _idx++;

//            if(_bytes[ _idx ] == OBJECT_END_SYMBOL)
//            {
//                cp.NameStartIndex = _paramName.startIndex;
//                cp.NameEndIndex = _paramName.endIndex;

//                return cp;
//            }

//            _paramName = ParseName();

//            while(IsWhiteSpace( _bytes[ _idx ] ) || _bytes[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
//                _idx++;

//            int startIndex;
//            for(; true; _idx++)
//            {
//                while(IsWhiteSpace( _bytes[ _idx ] ))
//                    _idx++;

//                switch(_bytes[ _idx ])
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        _idx++;
//                        var paramName2 = _paramName;
//                        _paramName = (0,0);

//                        var result = ParseObject();
//                        parsedParams.Add( new ComplexParam()
//                        {
//                            //Name = Encoding.UTF8.GetString( paramName2 ),
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
//                        _idx++;

//                        var paramname2 = _paramName;
//                        _paramName = (0,0);

//                        var result = ParseArray();
//                        //result.Name = Encoding.UTF8.GetString( paramname2 );
//                        parsedParams.Add( result );
//                        break;
//                    }

//                    case PARAMS_DELIMITER:
//                    {
//                        _idx++;
//                        _paramName = ParseName();
//                        //_paramValue = String.Empty;
//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        _idx++;

//                        var value = ParseQuotation();
//                        var simpleParam = new ParseJsonComplexParam( _bytesRaw )
//                        {
//                            ValueStartIndex = value.startIndex,
//                            ValueEndIndex = value.endIndex
//                            //Name = Encoding.UTF8.GetString( _paramName ),
//                            //Value = value.ToArray()
//                        };

//                        parsedParams.Add( simpleParam );
//                        break;
//                    }

//                    default:
//                    {
//                        while(IsWhiteSpace( _bytes[ _idx ] ))
//                            _idx++;

//                        startIndex = _idx;
//                        for(; true; _idx++)
//                        {
//                            if(IsWhiteSpace( _bytes[ _idx ] ))
//                                break;

//                            if(_bytes[ _idx ] == PARAMS_DELIMITER)
//                                break;
//                        }

//                        //if(_bytes[ startIndex.._idx ].SequenceEqual( NULL ))
//                        //{
//                        //    parsedParams.Add( new SimpleParam() { Name = Encoding.UTF8.GetString( _paramName ), Value = null } );
//                        //}
//                        //else if(_bytes[ startIndex.._idx ].SequenceEqual( FALSE ))
//                        //{
//                        //    parsedParams.Add( new BooleanParam() { Name = Encoding.UTF8.GetString( _paramName ), Value = Boolean.FalseString } );
//                        //}
//                        //else if(_bytes[ startIndex.._idx ].SequenceEqual( TRUE ))
//                        //{
//                        //    parsedParams.Add( new BooleanParam() { Name = Encoding.UTF8.GetString( _paramName ), Value = Boolean.TrueString } );
//                        //}
//                        //else
//                        //var value = _bytes[ startIndex.._idx ];
//                        parsedParams.Add( new ParseJsonComplexParam( _bytesRaw )
//                        {
//                            ValueStartIndex = startIndex,
//                            ValueEndIndex = _idx
//                            //Name = Encoding.UTF8.GetString( _paramName ),
//                            //Value = value
//                        } );

//                        _idx++;
//                        goto label;
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{OBJECT_END_SYMBOL}'" );
//        }

//        private ArrayParam ParseArray()
//        {
//            var items = new ArrayParam();

//            for(; true; _idx++)
//            {
//                while(IsWhiteSpace( _bytes[ _idx ] ))
//                    _idx++;

//                switch(_bytes[ _idx ])
//                {
//                    case OBJECT_START_SYMBOL:
//                    {
//                        _idx++;

//                        var complexParam = ParseObject();
//                        items.Add( complexParam );

//                        break;
//                    }

//                    case ARRAY_START_SYMBOL:
//                    {
//                        _idx++;

//                        var result = ParseArray();
//                        items.Add( result );

//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        _idx++;

//                        var value = ParseQuotation();
//                        //if(value.SequenceEqual( NULL ))
//                        //{
//                        //    items.Add( SimpleParam.ANONYMOUS_NULL );
//                        //}
//                        //else if(value.SequenceEqual( FALSE ))
//                        //{
//                        //    items.Add( BooleanParam.ANONYMOUS_FALSE );
//                        //}
//                        //else if(value.SequenceEqual( TRUE ))
//                        //{
//                        //    items.Add( BooleanParam.ANONYMOUS_TRUE );
//                        //}
//                        //else
//                        {

//                            items.Add( new ParseJsonComplexParam( _bytesRaw )
//                            {
//                                ValueStartIndex = value.startIndex,
//                                ValueEndIndex = value.endIndex
//                                //    Value = value.ToArray()
//                            } );
//                        }

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
//                        var value = ParseValue();
//                        //if(value.SequenceEqual( NULL ))
//                        //{
//                        //    items.Add( SimpleParam.ANONYMOUS_NULL );
//                        //}
//                        //else if(value.SequenceEqual( FALSE ))
//                        //{
//                        //    items.Add( BooleanParam.ANONYMOUS_FALSE );
//                        //}
//                        //else if(value.SequenceEqual( TRUE ))
//                        //{
//                        //    items.Add( BooleanParam.ANONYMOUS_TRUE );
//                        //}
//                        //else
//                        //{
//                            items.Add( new ParseJsonComplexParam( _bytesRaw )
//                            {
//                                ValueStartIndex = value.startIndex,
//                                ValueEndIndex = value.endIndex
//                                //Value = value.ToArray()
//                            } );
//                        //}

//                        _idx--;

//                        break;
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
//        }

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private (int startIndex, int endIndex) ParseQuotation()
//        {
//            bool escapeSymbols = false;
//            bool unicodeSymbols = false;

//            int startIndex = _idx;
//            for(; true; _idx++)
//            {
//                switch(_bytes[ _idx ])
//                {
//                    case ESCAPE_SYMBOL:
//                    {
//                        ++_idx;

//                        escapeSymbols = true;

//                        if(_bytes[ _idx ] == 'u')
//                            unicodeSymbols = true;

//                        break;
//                    }

//                    case QUOTE_SYMBOL:
//                    {
//                        //var quotation = _bytes[ startIndex.._idx ];

//                        //if(escapeSymbols)
//                        //{
//                        //    quotation = quotation
//                        //        .Replace( @"\b", "\b" )
//                        //        .Replace( @"\f", "\f" )
//                        //        .Replace( @"\n", "\n" )
//                        //        .Replace( @"\r", "\r" )
//                        //        .Replace( @"\t", "\t" )
//                        //        .Replace( @"\\", "\\" )
//                        //        .Replace( @"\""", "\"" );

//                        //    if(unicodeSymbols)
//                        //    {
//                        //        int unicodeCharIndex = quotation.IndexOf( @"\u" );
//                        //        while(unicodeCharIndex > -1)
//                        //        {
//                        //            string unicodeLiteral = quotation.Substring( unicodeCharIndex, 6 );
//                        //            int code = Int32.Parse( unicodeLiteral.Substring( 2 ), System.Globalization.NumberStyles.HexNumber );
//                        //            string unicodeChar = Char.ConvertFromUtf32( code );
//                        //            quotation = quotation.Replace( unicodeLiteral, unicodeChar );

//                        //            unicodeCharIndex = quotation.IndexOf( @"\u" );
//                        //        }
//                        //    }
//                        //}

//                        //return quotation;
//                        return (startIndex, _idx);
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
//        }

//        private static byte[] NULL = "null"u8.ToArray();
//        private static byte[] TRUE = "true"u8.ToArray();
//        private static byte[] FALSE = "false"u8.ToArray();

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private (int startIndex, int endIndex) ParseValue()
//        {
//            int startIndex = _idx;

//            while(IsWhiteSpace( _bytes[ _idx ] ))
//            {
//                startIndex++;
//                _idx++;
//            }

//            for(; true; _idx++)
//            {
//                if(IsWhiteSpace( _bytes[ _idx ] ))
//                    return (startIndex, _idx);

//                switch(_bytes[ _idx ])
//                {
//                    case PARAMS_DELIMITER:
//                    case OBJECT_END_SYMBOL:
//                    case ARRAY_END_SYMBOL:
//                    {
//                        return (startIndex, _idx);
//                    }
//                }
//            }

//            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
//        }

//        [MethodImpl( MethodImplOptions.AggressiveInlining )]
//        private (int startIndex, int endIndex) ParseName()
//        {
//            while(IsWhiteSpace( _bytes[ _idx ] ))
//                _idx++;

//            switch(_bytes[ _idx ])
//            {
//                case QUOTE_SYMBOL:

//                    _idx++;
//                    var result = ParseQuotation();

//                    for(_idx++; true; _idx++)
//                    {
//                        if(_bytes[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
//                            break;
//                    }

//                    return result;

//                default:
//                {
//                    int startIndex = _idx;
//                    for(; true; _idx++)
//                    {
//                        if(IsWhiteSpace( _bytes[ _idx ] ))
//                        {
//                            var result2 = (startIndex, _idx);

//                            for(_idx++; true; _idx++)
//                            {
//                                if(_bytes[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
//                                    break;
//                            }

//                            return result2;
//                        }

//                        if(_bytes[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
//                            return (startIndex, _idx);
//                    }
//                }
//            }
//        }
//    }
//#endif
//}
