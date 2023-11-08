#if NET5_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    public class JsonParserUsingReadonlySpanAdapter : IParser
    {
        public IParsedParam Parse( string text )
        {
            return new JsonParserUsingReadonlySpan( text ).Parse();
        }
    }

    internal ref struct JsonParserUsingReadonlySpan
    {
        private readonly string _text;
        private readonly ReadOnlySpan<char> _textSpan;
        private int _idx = 0;

        private const char OBJECT_START_SYMBOL = '{';
        private const char OBJECT_END_SYMBOL = '}';
        private const char ARRAY_START_SYMBOL = '[';
        private const char ARRAY_END_SYMBOL = ']';
        private const char PARAM_NAME_VALUE_DELIMITER = ':';
        private const char PARAMS_DELIMITER = ',';
        private const char QUOTE_SYMBOL = '"';
        private const char ESCAPE_SYMBOL = '\\';

        //private const string NULL = "null";
        //private const string FALSE = "false";
        //private const string TRUE = "true";

        public JsonParserUsingReadonlySpan( string text )
        {
            _text = text;
            _textSpan = text;
        }

        public IParsedParam Parse()
        {
            while(_textSpan[ _idx ].IsWhiteSpace())
                _idx++;

            switch(_textSpan[ _idx ])
            {
                case OBJECT_START_SYMBOL: _idx++; return ParseObject();
                case ARRAY_START_SYMBOL: _idx++; return ParseArray();

                default: throw new Exception( $"Unexpected symbol '{_textSpan[ _idx ]}' at position {_idx}" );
            }
        }

        private ComplexParam ParseObject()
        {
            var cp = new ComplexParam()
            {
                Name = String.Empty,
            };

            bool isParsingParamName = true;
            string paramName = String.Empty;

            for(; _idx < _textSpan.Length; _idx++)
            {
                while(_textSpan[ _idx ].IsWhiteSpace())
                    _idx++;

                switch(_textSpan[ _idx ])
                {
                    case OBJECT_START_SYMBOL:
                    {
                        _idx++;

                        var result = ParseObject();
                        result.Name = paramName;

                        //cp.SubParams.Add( result );
                        cp.Complex.Add( result );

                        isParsingParamName = true;
                        break;
                    }

                    case OBJECT_END_SYMBOL:
                    {
                        return cp;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        _idx++;

                        var result = ParseArray();
                        result.Name = paramName;
                        //cp.SubParams.Add( result );
                        cp.Arrays.Add( result );

                        isParsingParamName = true;
                        break;
                    }

                    case PARAMS_DELIMITER:
                    {
                        isParsingParamName = true;
                        break;
                    }

                    default:
                    {
                        if(isParsingParamName)
                        {
                            paramName = ParseName();
                            isParsingParamName = false;
                            break;
                        }
                        else
                        {
                            var sp = new SimpleParam() { Name = paramName };

                            if(_textSpan[ _idx ] == QUOTE_SYMBOL)
                            {
                                _idx++;
                                sp.Value = ParseQuotation();
                            }
                            else
                            {
                                sp = ParseValue();
                                sp.Name = paramName;
                            }

                            //cp.SubParams.Add( sp );
                            cp.Simples.Add( sp );

                            isParsingParamName = true;
                            break;
                        }
                    }
                }
            }

            return cp;
        }

        private ArrayParam ParseArray()
        {
            var items = new ArrayParam();

            for(; _idx < _textSpan.Length; _idx++)
            {
                if(_textSpan[ _idx ].IsWhiteSpace())
                    continue;

                switch(_textSpan[ _idx ])
                {
                    case OBJECT_START_SYMBOL:
                    {
                        _idx++;

                        var complexParam = ParseObject();

                        items.Complex.Add( complexParam );

                        break;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        _idx++;

                        var result = ParseArray();
                        items.Arrays.Add( result );

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        _idx++;

                        var parseQuotation = ParseQuotation();
                        items.Simples.Add( new SimpleParam() { Value = parseQuotation } );

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
                        SimpleParam sp = new SimpleParam();

                        if(_textSpan[ _idx ] == QUOTE_SYMBOL)
                        {
                            _idx++;
                            sp.Value = ParseQuotation();
                        }
                        else
                        {
                            sp = ParseValue();
                            //if(sp.Value == null)
                            //    sp = null;
                        }

                        items.Simples.Add( sp );
                        _idx--;
                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseName()
        {
            while(_textSpan[ _idx ].IsWhiteSpace())
                _idx++;

            switch(_textSpan[ _idx ])
            {
                case QUOTE_SYMBOL:

                    _idx++;
                    string paramName = ParseQuotation();

                    for(_idx++; true; _idx++)
                    {
                        if(_textSpan[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
                            break;
                    }

                    return paramName;

                case ARRAY_END_SYMBOL: return String.Empty;   //after , we search for a new param name but it might be missing and the element be done.
                case OBJECT_END_SYMBOL: return String.Empty; //after , we search for a new param name but it might be missing and the element be done.

                default:
                {
                    int startIndex = _idx;
                    for(; true; _idx++)
                    {
                        int lastNameCharIndex = _idx;

                        while(_textSpan[ _idx ].IsWhiteSpace())
                            _idx++;

                        if(_textSpan[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
                            return _textSpan[ startIndex..lastNameCharIndex ].ToString();
                    }
                }
            }
        }


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SimpleParam ParseValue()
        {
            static SimpleParam Getsimplevalue( ReadOnlySpan<char> readOnlySpan )
            {
                //if(readOnlySpan.SequenceEqual( "false" ))
                //    return new SimpleParam() { Value = Boolean.FalseString };
                //else if(readOnlySpan.SequenceEqual( "true" ))
                //    return new SimpleParam() { Value = Boolean.TrueString };
                if(readOnlySpan.SequenceEqual( "null" ))
                    return new SimpleParam() { Value = null };
                return new SimpleParam() { Value = readOnlySpan.ToString() };
            }

            int startIndex = _idx;

            for(; true; _idx++)
            {
                switch(_textSpan[ _idx ])
                {
                    case QUOTE_SYMBOL:
                    {
                        _idx++;
                        return new SimpleParam() { Value = ParseQuotation() };
                    }

                    default:
                    {
                        if(_textSpan[ _idx ].IsWhiteSpace())
                        {
                            startIndex++;
                            continue;
                        }

                        for(; _idx < _textSpan.Length; _idx++)
                        {
                            if(_textSpan[ _idx ].IsWhiteSpace())
                                return Getsimplevalue( _textSpan[ startIndex.._idx ] );

                            switch(_textSpan[ _idx ])
                            {
                                case PARAMS_DELIMITER:
                                case OBJECT_END_SYMBOL:
                                case ARRAY_END_SYMBOL:
                                {
                                    return Getsimplevalue( _textSpan[ startIndex.._idx ] );
                                }
                            }
                        }
                        break;
                    }
                }
            }

            throw new Exception( $"unxpected symbol" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseQuotation()
        {
            bool escapeSymbols = false;
            bool unicodeSymbols = false;

            int startIndex = _idx;
            for(; true; _idx++)
            {
                switch(_textSpan[ _idx ])
                {
                    case ESCAPE_SYMBOL:
                    {
                        ++_idx;

                        escapeSymbols = true;

                        if(_textSpan[ _idx ] == 'u')
                            unicodeSymbols = true;

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        if(escapeSymbols)
                        {
                            var quotation = _textSpan[ startIndex.._idx ];
                            if(unicodeSymbols)
                            {
                                StringBuilder sb = new StringBuilder();
                                var unicodeQuotation = _textSpan[ startIndex.._idx ];

                                int unicodeCharIndex = unicodeQuotation.IndexOf( @"\u" );
                                while(unicodeCharIndex > -1)
                                {
                                    sb.Append( unicodeQuotation[ 0..unicodeCharIndex ] );

                                    var unicodeLiteral = unicodeQuotation.Slice( unicodeCharIndex, 6 );
                                    int symbolCode = Int32.Parse( unicodeLiteral[ 2.. ], System.Globalization.NumberStyles.HexNumber );
                                    var unicodeChar = Char.ConvertFromUtf32( symbolCode );

                                    sb.Append( unicodeChar );

                                    unicodeQuotation = unicodeQuotation[ (unicodeCharIndex + 6).. ];
                                    unicodeCharIndex = unicodeQuotation.IndexOf( @"\u" );
                                }

                                return sb.ToString()
                                    .Replace( @"\b", "\b" )
                                    .Replace( @"\f", "\f" )
                                    .Replace( @"\n", "\n" )
                                    .Replace( @"\r", "\r" )
                                    .Replace( @"\t", "\t" )
                                    .Replace( @"\\", "\\" )
                                    .Replace( @"\""", "\"" );
                            }

                            return quotation.ToString()
                                .Replace( @"\b", "\b" )
                                .Replace( @"\f", "\f" )
                                .Replace( @"\n", "\n" )
                                .Replace( @"\r", "\r" )
                                .Replace( @"\t", "\t" )
                                .Replace( @"\\", "\\" )
                                .Replace( @"\""", "\"" );
                        }
                        else
                        {
                            return _textSpan[ startIndex.._idx ].ToString();
                        }
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }
    }
}
#endif
