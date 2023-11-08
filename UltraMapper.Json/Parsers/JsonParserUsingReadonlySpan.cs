﻿#if NET5_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Parameters2;

namespace UltraMapper.Json
{
    public class JsonParserUsingReadonlySpanAdapterP2 : IParser2
    {
        public IParsedParam2 Parse( string text )
        {
            return new JsonParserUsingReadonlySpanP2( text ).Parse();
        }
    }

    internal ref struct JsonParserUsingReadonlySpanP2
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

        public JsonParserUsingReadonlySpanP2( string text )
        {
            _text = text;
            _textSpan = text;
        }

        public IParsedParam2 Parse()
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

        private ComplexParam2 ParseObject()
        {
            var cp = new ComplexParam2( _text )
            {

            };

            bool isParsingParamName = true;
            int paramNameStartIndex = _idx;
            int paramNameLastIndex = -1;

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
                        result.NameStartIndex = paramNameStartIndex;
                        result.NameEndIndex = paramNameLastIndex;

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
                        result.NameStartIndex = paramNameStartIndex;
                        result.NameEndIndex = paramNameLastIndex;

                        //cp.SubParams.Add( result );
                        cp.Array.Add( result );

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
                            paramNameStartIndex = _idx;
                            paramNameLastIndex = ParseName();
                            isParsingParamName = false;
                            break;
                        }
                        else
                        {
                            var sp = new SimpleParam2( _text ) { NameStartIndex = paramNameStartIndex, NameEndIndex = paramNameLastIndex };

                            if(_textSpan[ _idx ] == QUOTE_SYMBOL)
                            {
                                _idx++;
                                sp.ValueStartIndex = _idx;
                                sp.ValueEndIndex = ParseQuotation( out bool escape, out bool unicode );
                            }
                            else
                            {
                                sp = ParseValue();
                                sp.NameStartIndex = paramNameStartIndex;
                                sp.NameEndIndex = paramNameLastIndex;
                            }

                            //cp.SubParams.Add( sp );
                            cp.Simple.Add( sp );

                            isParsingParamName = true;
                            break;
                        }
                    }
                }
            }

            return cp;
        }

        private ArrayParam2 ParseArray()
        {
            var items = new ArrayParam2( _text );

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
                        items.Array.Add( result );

                        break;
                    }

                    case QUOTE_SYMBOL:
                    {
                        _idx++;

                        var sp = new SimpleParam2( _text );
                        sp.ValueStartIndex = _idx;
                        sp.ValueEndIndex = ParseQuotation( out bool escape, out bool unicode );

                        items.Simple.Add( sp );

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
                        SimpleParam2 sp = new SimpleParam2( _text );

                        if(_textSpan[ _idx ] == QUOTE_SYMBOL)
                        {
                            _idx++;
                            sp.ValueStartIndex = _idx;
                            sp.ValueEndIndex = ParseQuotation( out bool escape, out bool unicode );
                        }
                        else
                        {
                            sp = ParseValue();
                            //if(sp.Value == null)
                            //    sp = null;
                        }

                        items.Simple.Add( sp );
                        _idx--;
                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int ParseName()
        {
            while(_textSpan[ _idx ].IsWhiteSpace())
                _idx++;

            switch(_textSpan[ _idx ])
            {
                case QUOTE_SYMBOL:

                    _idx++;

                    int endIndex = ParseQuotation(out bool escape, out bool unicode);

                    for(_idx++; true; _idx++)
                    {
                        if(_textSpan[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
                            break;
                    }

                    return endIndex;

                case ARRAY_END_SYMBOL: return -1;   //after , we search for a new param name but it might be missing and the element be done.
                case OBJECT_END_SYMBOL: return -1; //after , we search for a new param name but it might be missing and the element be done.

                default:
                {
                    int startIndex = _idx;
                    for(; true; _idx++)
                    {
                        int lastNameCharIndex = _idx;

                        while(_textSpan[ _idx ].IsWhiteSpace())
                            _idx++;

                        if(_textSpan[ _idx ] == PARAM_NAME_VALUE_DELIMITER)
                            return lastNameCharIndex;
                    }
                }
            }
        }


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SimpleParam2 ParseValue()
        {
            //static SimpleParam2 Getsimplevalue( ReadOnlySpan<char> readOnlySpan )
            //{
            //    //if(readOnlySpan.SequenceEqual( "false" ))
            //    //    return new SimpleParam() { Value = Boolean.FalseString };
            //    //else if(readOnlySpan.SequenceEqual( "true" ))
            //    //    return new SimpleParam() { Value = Boolean.TrueString };
            //    if(readOnlySpan.SequenceEqual( "null" ))
            //        return new SimpleParam2(_text) { Value = null };
            //    return new SimpleParam2(_Text) { Value = readOnlySpan.ToString() };
            //}

            int startIndex = _idx;

            for(; true; _idx++)
            {
                switch(_textSpan[ _idx ])
                {
                    case QUOTE_SYMBOL:
                    {
                        _idx++;
                        return new SimpleParam2( _text ) { ValueStartIndex = startIndex, ValueEndIndex = ParseQuotation( out bool escape, out bool unicode ) };
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
                                return new SimpleParam2( _text ) { ValueStartIndex = startIndex, ValueEndIndex = _idx };

                            switch(_textSpan[ _idx ])
                            {
                                case PARAMS_DELIMITER:
                                case OBJECT_END_SYMBOL:
                                case ARRAY_END_SYMBOL:
                                {
                                    return new SimpleParam2( _text ) { ValueStartIndex = startIndex, ValueEndIndex = _idx };
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
        private int ParseQuotation( out bool escapeSymbols, out bool unicodeSymbols )
        {
            escapeSymbols = false;
            unicodeSymbols = false;

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
                        return _idx;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
        }
    }
}
#endif
