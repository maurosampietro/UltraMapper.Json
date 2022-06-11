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

        private string _paramValue = String.Empty;
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
            for( int i = 0; true; i++ )
            {
                if( IsWhiteSpace( text[ i ] ) )
                    continue;

                switch( text[ i ] )
                {
                    case OBJECT_START_SYMBOL:
                    {
                        i++;
                        return ParseObject( text, ref i );
                    }

                    case ARRAY_START_SYMBOL:
                    {
                        i++;
                        return ParseArray( text, ref i );
                    }

                    default:
                        throw new Exception( $"Unexpected symbol '{text[ i ]}' at position {i}" );
                }
            }

            throw new Exception( $"Expected symbol '{OBJECT_START_SYMBOL}' or '{ARRAY_START_SYMBOL}'" );
        }

        private ComplexParam ParseObject( string text, ref int i )
        {
            var parsedParams = new List<IParsedParam>( MIN_CAPACITY );

        label:
            for( ; true; i++ )
            {
                if( IsWhiteSpace( text[ i ] ) )
                    continue;

                if( text[ i ] == OBJECT_END_SYMBOL )
                {
                    return new ComplexParam()
                    {
                        Name = _paramName,
                        SubParams = parsedParams
                    };
                }

                _paramName = ParseName( text, ref i );

                while( IsWhiteSpace( text[ i ] ) || text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                    i++;

                int startIndex;
                for( ; true; i++ )
                {
                    if( IsWhiteSpace( text[ i ] ) )
                        continue;

                    switch( text[ i ] )
                    {
                        case OBJECT_START_SYMBOL:
                        {
                            i++;
                            string paramName2 = _paramName;
                            _paramName = String.Empty;

                            var result = ParseObject( text, ref i );
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
                                SubParams = parsedParams
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

                        case PARAMS_DELIMITER:
                        {
                            i++;
                            _paramName = ParseName( text, ref i );
                            _paramValue = String.Empty;
                            break;
                        }

                        case QUOTE_SYMBOL:
                        {
                            i++;

                            var simpleParam = new SimpleParam()
                            {
                                Name = _paramName,
                                Value = ParseQuotation( text, ref i )
                            };

                            parsedParams.Add( simpleParam );
                            break;
                        }

                        default:
                        {
                            while( IsWhiteSpace( text[ i ] ) )
                                i++;

                            startIndex = i;
                            for( ; true; i++ )
                            {
                                if( IsWhiteSpace( text[ i ] ) )
                                    break;

                                if( text[ i ] == PARAMS_DELIMITER )
                                    break;
                            }

                            parsedParams.Add( new SimpleParam()
                            {
                                Name = _paramName,
                                Value = text.Substring( startIndex, i - startIndex )
                            } );

                            i++;
                            goto label;
                        }
                    }
                }

            }

            throw new Exception( $"Expected symbol '{OBJECT_END_SYMBOL}'" );
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
                            Value = ParseArrayValue( text, ref i )
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

                default:
                {
                    int startIndex = i;
                    for( ; true; i++ )
                    {
                        if( IsWhiteSpace( text[ i ] ) )
                        {
                            _paramName = text.Substring( startIndex, i - startIndex );

                            for( i++; true; i++ )
                            {
                                if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                                    break;
                            }

                            return _paramName;
                        }

                        if( text[ i ] == PARAM_NAME_VALUE_DELIMITER )
                            return text.Substring( startIndex, i - startIndex );
                    }
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private string ParseArrayValue( string text, ref int i )
        {
            int startIndex = i;

            for( ; i < text.Length; i++ )
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
            }

            throw new Exception( $"Expected symbol '{QUOTE_SYMBOL}'" );
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