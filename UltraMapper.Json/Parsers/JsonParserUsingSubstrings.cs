﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UltraMapper.Parsing;

namespace UltraMapper.Json
{
    internal class JsonParserUsingSubstrings : IParser
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

        private const int MIN_CAPACITY = 16;

        private string _paramName = String.Empty;

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
            int i = 0;
            while( IsWhiteSpace( text[ i ] ) )
                i++;

            char c = text[ i ];
            switch( c )
            {
                case OBJECT_START_SYMBOL: i++; return ParseObject( text, ref i );
                case ARRAY_START_SYMBOL: i++; return ParseArray( text, ref i );

                default: throw new Exception( $"Unexpected symbol '{c}' at position {i}" );
            }
        }

        private ComplexParam ParseObject( string text, ref int i )
        {
            var cp = new ComplexParam()
            {
                Name = String.Empty,
                SubParams = new List<IParsedParam>( MIN_CAPACITY )
            };

            bool isParsingParamName = true;
            string paramName = String.Empty;

            for( ; i < text.Length; i++ )
            {
                while( IsWhiteSpace( text[ i ] ) )
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
                            var value = ParseValue( text, ref i );
                            cp.SubParams.Add( new SimpleParam() { Name = paramName, Value = value } );

                            isParsingParamName = true;
                            break;
                        }
                    }
                }
            }

            return cp;
        }

        private ArrayParam ParseArray( string text, ref int i )
        {
            var items = new ArrayParam();

            for( ; true; i++ )
            {
                if( IsWhiteSpace( text[ i ] ) )
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
        private string ParseName( string text, ref int i )
        {
            while( IsWhiteSpace( text[ i ] ) )
                i++;

            switch( text[ i ] )
            {
                case QUOTE_SYMBOL:

                    i++;
                    _paramName = ParseQuotation( text, ref i );

                    for( i++; true; i++ )
                    {
                        if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                            break;
                    }

                    return _paramName;

                case ARRAY_END_SYMBOL: return String.Empty;   //after , we search for a new param name but it might be missing and the element be done.
                case OBJECT_END_SYMBOL: return String.Empty; //after , we search for a new param name but it might be missing and the element be done.

                default:
                {
                    int startIndex = i;
                    for( ; true; i++ )
                    {
                        int lastNameCharIndex = i;

                        while( IsWhiteSpace( text[ i ] ) )
                            i++;

                        if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                            return text.Substring( startIndex, lastNameCharIndex - startIndex );
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseValue( string text, ref int i )
        {
            int startIndex = i;

            for( ; true; i++ )
            {
                switch( text[ i ] )
                {
                    case QUOTE_SYMBOL:
                    {
                        i++;
                        return ParseQuotation( text, ref i );
                    }

                    default:
                    {
                        if( IsWhiteSpace( text[ i ] ) )
                        {
                            startIndex++;
                            continue;
                        }

                        for( ; i < text.Length; i++ )
                        {
                            if( IsWhiteSpace( text[ i ] ) )
                                return text.Substring( startIndex, i - startIndex );

                            switch( text[ i ] )
                            {
                                case PARAMS_DELIMITER:
                                case OBJECT_END_SYMBOL:
                                case ARRAY_END_SYMBOL:
                                {
                                    return text.Substring( startIndex, i - startIndex );
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
        private string ParseQuotation( string text, ref int i )
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
                        var quotation = text.Substring( startIndex, i - startIndex );

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
    }
}