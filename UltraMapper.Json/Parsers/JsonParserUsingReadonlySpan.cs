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

        private const string NULL = "null";
        private const string FALSE = "false";
        private const string TRUE = "true";

        private const StringComparison _compareMode =
            StringComparison.InvariantCultureIgnoreCase;

        private string _text;

        public IParsedParam Parse( string text )
        {
            _text = text;

            int i = 0;
            while( text[ i ].IsWhiteSpace() )
                i++;

            switch( text[ i ] )
            {
                case OBJECT_START_SYMBOL: i++; return ParseObject( text, ref i );
                case ARRAY_START_SYMBOL: i++; return ParseArray( text, ref i );

                default: throw new Exception( $"Unexpected symbol '{text[ i ]}' at position {i}" );
            }
        }

        private ComplexParam ParseObject( ReadOnlySpan<char> text, ref int i )
        {
            var cp = new ComplexParam()
            {
                Name = String.Empty,
                SubParams = new List<IParsedParam>()
            };

            bool isParsingParamName = true;
            string paramName = String.Empty;

            for( ; i < text.Length; i++ )
            {
                while( text[ i ].IsWhiteSpace() )
                    i++;

                switch( text[ i ] )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;

                        var result = ParseObject( text, ref i );
                        cp.SubParams.Add( new ComplexParam()
                        {
                            Name = paramName,
                            SubParams = result.SubParams
                        } );
                        isParsingParamName = true;
                        break;
                    }

                    case OBJECT_END_SYMBOL:
                    {
                        return cp;
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;

                        var result = ParseArray( text, ref i );
                        result.Name = paramName;
                        cp.SubParams.Add( result );

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
                        if( isParsingParamName )
                        {
                            paramName = ParseName( text, ref i );
                            isParsingParamName = false;
                            break;
                        }
                        else
                        {
                            var sp = new SimpleParam() { Name = paramName };

                            if( text[ i ] == QUOTE_SYMBOL )
                            {
                                i++;
                                sp.Value = ParseQuotation( text, ref i );
                            }
                            else
                            {
                                sp = ParseValue( text, ref i );
                                sp.Name = paramName;
                            }

                            cp.SubParams.Add( sp );

                            isParsingParamName = true;
                            break;
                        }
                    }
                }
            }

            return cp;
        }

        private ArrayParam ParseArray( ReadOnlySpan<char> text, ref int i )
        {
            var items = new ArrayParam();

            for( ; i < text.Length; i++ )
            {
                if( text[ i ].IsWhiteSpace() )
                    continue;

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
                        SimpleParam sp = new SimpleParam();

                        if( text[ i ] == QUOTE_SYMBOL )
                        {
                            i++;
                            sp.Value = ParseQuotation( text, ref i );
                        }
                        else
                        {
                            sp = ParseValue( text, ref i );
                        }

                        items.Add( sp );
                        i--;
                        break;
                    }
                }
            }

            throw new Exception( $"Expected symbol '{ARRAY_END_SYMBOL}'" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseName( ReadOnlySpan<char> text, ref int i )
        {
            while( text[ i ].IsWhiteSpace() )
                i++;

            switch( text[ i ] )
            {
                case QUOTE_SYMBOL:

                    i++;
                    string paramName = ParseQuotation( text, ref i );

                    for( i++; true; i++ )
                    {
                        if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                            break;
                    }

                    return paramName;

                case ARRAY_END_SYMBOL: return String.Empty;   //after , we search for a new param name but it might be missing and the element be done.
                case OBJECT_END_SYMBOL: return String.Empty; //after , we search for a new param name but it might be missing and the element be done.

                default:
                {
                    int startIndex = i;
                    for( ; true; i++ )
                    {
                        int lastNameCharIndex = i;

                        while( text[ i ].IsWhiteSpace() )
                            i++;

                        if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                            return text[ startIndex..lastNameCharIndex ].ToString();
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SimpleParam ParseValue( ReadOnlySpan<char> text, ref int i )
        {
            static SimpleParam getReturnValue( ReadOnlySpan<char> value, int startIndex, int i )
            {
                if( value.Equals( NULL, _compareMode ) )
                    return new SimpleParam() { Value = null };

                if( value.Equals( FALSE, _compareMode ) )
                    return new BooleanParam() { Value = Boolean.FalseString };

                if( value.Equals( TRUE, _compareMode ) )
                    return new BooleanParam() { Value = Boolean.TrueString };

                return new SimpleParam(){ Value = value.ToString() };
            };
       
            int startIndex = i;

            for( ; true; i++ )
            {
                switch( text[ i ] )
                {
                    case QUOTE_SYMBOL:
                    {
                        i++;
                        return new SimpleParam() { Value = ParseQuotation( text, ref i ) };
                    }

                    default:
                    {
                        if( text[ i ].IsWhiteSpace() )
                        {
                            startIndex++;
                            continue;
                        }

                        for( ; i < text.Length; i++ )
                        {
                            if( text[ i ].IsWhiteSpace() )
                                return getReturnValue( text[ startIndex..i ], startIndex, i );

                            switch( text[ i ] )
                            {
                                case PARAMS_DELIMITER:
                                case OBJECT_END_SYMBOL:
                                case ARRAY_END_SYMBOL:
                                {
                                    return getReturnValue( text[ startIndex..i ], startIndex, i );
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
        private string ParseQuotation( ReadOnlySpan<char> text, ref int i )
        {
            bool escapeSymbols = false;
            bool unicodeSymbols = false;

            int startIndex = i;
            for( ; true; i++ )
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
                                    int code = Int32.Parse( unicodeLiteral[ 2.. ], System.Globalization.NumberStyles.HexNumber );
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
    }
}
#endif
