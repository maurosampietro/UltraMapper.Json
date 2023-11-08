//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using UltraMapper.Parsing;

//namespace UltraMapper.Json.Parsers.other_parsers
//{
//    public class IJsonREaderRefStructAdapter : IParser
//    {
//        public IParsedParam Parse( string text )
//        {
//            return new JsonRefStructUtf8Reader( text ).Parse();
//        }
//    }

//    public ref struct JsonRefStructUtf8Reader
//    {
//        private readonly ReadOnlySpan<byte> _bytesSpan;
//        private readonly byte[] _bytes;
//        private int _idx = 0;
//        private IParsedParam _rootParam;

//        public JsonRefStructUtf8Reader( string text )
//        {
//            _bytes = Encoding.UTF8.GetBytes( text );
//            _bytesSpan = _bytes.AsSpan();
//        }

//        private const byte OPEN_OBJECT = (byte)'{';
//        private const byte OPEN_ARRAY = (byte)'[';
//        private const byte CLOSE_OBJECT = (byte)'}';
//        private const byte CLOSE_ARRAY = (byte)']';
//        private const byte PARAM_NAME_VALUE_SEPARATOR = (byte)':';
//        private const byte PARAM_SEPARATOR = (byte)',';
//        private const char QUOTE = '"';
//        private static byte[] NULL = "null"u8.ToArray();
//        private static byte[] TRUE = "true"u8.ToArray();
//        private static byte[] FALSE = "false"u8.ToArray();

//        private static bool IsWhitespace( byte c )
//        {
//            return c is 32 or 9 or 10 or 13;
//        }

//        public IParsedParam Parse()
//        {
//            for(; _idx < _bytesSpan.Length; _idx++)
//            {
//                byte value = _bytesSpan[ _idx ];
//                if(IsWhitespace( value )) continue;

//                if(value == OPEN_OBJECT)
//                {
//                    var cp = new ComplexParam() { Name = String.Empty };
//                    _rootParam = cp;
//                    ParseObject( cp );
//                }
//                else if(value == OPEN_ARRAY)
//                {
//                    var ap = new ArrayParam() { Name = String.Empty };
//                    _rootParam = ap;
//                    ParseArray( ap );
//                }
//                else if(_idx + 4 < _bytesSpan.Length && _bytesSpan.Slice( _idx, _idx + 4 ).SequenceEqual( NULL ))
//                {
//                    Console.WriteLine( "NULL" );
//                    _idx += 4;
//                }
//                else if(_idx + 5 < _bytesSpan.Length)
//                {
//                    if(_bytesSpan.Slice( _idx, _idx + 5 ).SequenceEqual( TRUE ))
//                    {
//                        Console.WriteLine( "TRUE" );
//                        _idx += 5;
//                    }
//                    else if(_bytesSpan.Slice( _idx, _idx + 5 ).SequenceEqual( FALSE ))
//                    {
//                        Console.WriteLine( "FALSE" );
//                        _idx += 5;
//                    }
//                }
//            }

//            return _rootParam;
//        }

//        private void ParseArray( ArrayParam array )
//        {
//            for(_idx++; _idx < _bytesSpan.Length; _idx++)
//            {
//                var value = _bytesSpan[ _idx ];
//                if(value == OPEN_OBJECT)
//                {
//                    var cp = new ComplexParam();
//                    array.Add( cp );
//                    ParseObject( cp );
//                }
//                else if(value == OPEN_ARRAY)
//                {
//                    var ap = new ArrayParam();
//                    array.Add( ap );
//                    ParseArray( ap );
//                }
//                else if(value == CLOSE_ARRAY) { _idx++; return; }
//                else if(!IsWhitespace( value ))
//                {
//                    var propValue = ReadValue();

//                    if(propValue.SequenceEqual( NULL ))
//                    {
//                        array.Add( SimpleParam.ANONYMOUS_NULL );
//                    }
//                    else if(propValue.SequenceEqual( TRUE ))
//                    {
//                        array.Add( BooleanParam.ANONYMOUS_TRUE );
//                    }
//                    else if(propValue.SequenceEqual( FALSE ))
//                    {
//                        array.Add( BooleanParam.ANONYMOUS_FALSE );
//                    }
//                    else
//                    {
//                        array.Add( new SimpleParam()
//                        {
//                            Value = Encoding.UTF8.GetString( propValue ).Trim( QUOTE ),
//                        } );
//                    }


//                    for(; _idx < _bytesSpan.Length; _idx++)
//                    {
//                        if(_bytesSpan[ _idx ] == PARAM_SEPARATOR)
//                        {
//                            //_idx++;
//                            break;
//                        }

//                        else if(_bytesSpan[ _idx ] == CLOSE_ARRAY)
//                        {
//                            _idx++;

//                            for(; _idx < _bytesSpan.Length; _idx++)
//                            {
//                                if(!IsWhitespace( _bytesSpan[ _idx ] ) && _bytesSpan[ _idx ] != PARAM_SEPARATOR)
//                                {
//                                    _idx--;
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }
//                else if(value == CLOSE_ARRAY)
//                {
//                    return;
//                }
//            }
//        }

//        private void ParseObject( ComplexParam cp )
//        {
//            int entryIdx = ++_idx;
//            int currentParamxIdx = entryIdx;

//            for(; _idx < _bytesSpan.Length; _idx++)
//            {
//                byte value = _bytesSpan[ _idx ];
//                if(value != PARAM_NAME_VALUE_SEPARATOR) continue;

//                var propName = ReadPropertyNameOrValueToken( currentParamxIdx );
//                string temp = Encoding.UTF8.GetString( propName );

//                for(_idx++; _idx < _bytesSpan.Length; _idx++)
//                {
//                    value = _bytesSpan[ _idx ];
//                    if(value == OPEN_OBJECT)
//                    {
//                        var cp2 = new ComplexParam
//                        {
//                            Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                        };
//                        cp.SubParams.Add( cp2 );
//                        ParseObject( cp2 );
//                        currentParamxIdx = _idx;
                        
//                    }
//                    else if(value == OPEN_ARRAY)
//                    {
//                        var ap = new ArrayParam()
//                        {
//                            Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                        };
//                        cp.SubParams.Add( ap );
//                        ParseArray( ap );
//                        currentParamxIdx = _idx;
                        
//                    }
//                    else if(value == CLOSE_OBJECT) { break; }
//                    else if(!IsWhitespace( value ))
//                    {
//                        var propValue = ReadValue();

//                        if(propValue.SequenceEqual( NULL ))
//                        {
//                            cp.SubParams.Add( new SimpleParam()
//                            {
//                                Value = null,
//                                Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                            } );
//                        }
//                        else if(propValue.SequenceEqual( TRUE ))
//                        {
//                            cp.SubParams.Add( new BooleanParam()
//                            {
//                                Value = Encoding.UTF8.GetString( TRUE ),
//                                Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                            } );
//                        }
//                        else if(propValue.SequenceEqual( FALSE ))
//                        {
//                            cp.SubParams.Add( new BooleanParam()
//                            {
//                                Value = Encoding.UTF8.GetString( FALSE ),
//                                Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                            } );
//                        }
//                        else
//                        {
//                            cp.SubParams.Add( new SimpleParam()
//                            {
//                                Value = Encoding.UTF8.GetString( propValue ).Trim( QUOTE ),
//                                Name = Encoding.UTF8.GetString( propName ).Trim( QUOTE )
//                            } );
//                        }



//                        for(; _idx < _bytesSpan.Length; _idx++)
//                        {
//                            if(_bytesSpan[ _idx ] == PARAM_SEPARATOR)
//                            {
//                                _idx++;
//                                break;
//                            }

//                            else if(_bytesSpan[ _idx ] == CLOSE_OBJECT)
//                            {
//                                _idx++;
//                                return;
//                            }
//                        }

//                        currentParamxIdx = _idx;
//                        break;
//                    }
//                }
//            }
//        }

//        private byte[] ReadValue()
//        {
//            int entryIdx = _idx;

//            for(; _idx < _bytesSpan.Length; _idx++)
//            {
//                byte value = _bytesSpan[ _idx ];

//                if(value != CLOSE_ARRAY && value != CLOSE_OBJECT &&
//                  value != PARAM_SEPARATOR) continue;

//                return ReadPropertyNameOrValueToken( entryIdx );
//            }

//            throw new Exception();
//        }

//        private readonly byte[] ReadPropertyNameOrValueToken( int idx )
//        {
//            TrimIndexes( idx, out int startIdx, out int endIdx );

//            int len = endIdx - startIdx;
//            byte[] token = new byte[ len ];
//            Buffer.BlockCopy( _bytes, startIdx, token, 0, len );

//            Console.WriteLine( Encoding.UTF8.GetString( token ) );
//            return token;
//        }

//        //Move start and end indexes in order to not include whitespaces
//        private readonly void TrimIndexes( int idx, out int startIdx, out int endIdx )
//        {
//            startIdx = idx;
//            for(; startIdx < _idx; startIdx++)
//            {
//                if(!IsWhitespace( _bytesSpan[ startIdx ] )) break;
//            }

//            endIdx = _idx - 1;
//            //avoid unwanted last decrement
//            for(; endIdx > idx; endIdx--)
//            {
//                if(!IsWhitespace( _bytesSpan[ endIdx ] )) break;
//            }
//            endIdx++;
//        }
//    }
//}
