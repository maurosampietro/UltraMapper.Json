using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Parameters2;

namespace UltraMapper.Json.Parsers
{
#if NET7_0_OR_GREATER
    public sealed class JsonParserUsingReadonlySpanAdapterP2AvxSupported : IParser
    {
        public IParsedParam Parse( string text )
        {
            return new JsonParserUsingReadonlySpanP2AvxSupported( text ).Parse();
        }
    }

    internal ref struct JsonParserUsingReadonlySpanP2AvxSupported
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

        public JsonParserUsingReadonlySpanP2AvxSupported( string text )
        {
            text = AVX2Utils.GetStringWithoutWhitespaces( text );

            _text = text;
            _textSpan = text;
        }

        public IParsedParam Parse()
        {
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
                            paramNameLastIndex = ParseName( out paramNameStartIndex );
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
                                sp.ContainsEscapedChars = escape;
                                sp.ContainsLiteralUnicodeChars = unicode;
                            }
                            else
                            {
                                sp = ParseValue();
                                sp.NameStartIndex = paramNameStartIndex;
                                sp.NameEndIndex = paramNameLastIndex;
                                if(sp.Value == "null")
                                    sp.ValueStartIndex = -1;
                            }

                            //cp.SubParams.Add( sp );
                            cp.Simple.Add( sp );
                            if(_textSpan[ _idx ] == OBJECT_END_SYMBOL)
                                return cp;

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
                        sp.ContainsEscapedChars = escape;
                        sp.ContainsLiteralUnicodeChars = unicode;
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
                            sp.ContainsEscapedChars = escape;
                            sp.ContainsLiteralUnicodeChars = unicode;
                        }
                        else
                        {
                            sp = ParseValue();

                            if(sp.Value == "null")
                                sp.ValueStartIndex = -1;
                            //if(sp.Value == null)
                            //    sp = null;

                        }

                        items.Simple.Add( sp );
                        if(_textSpan[ _idx ] == ARRAY_END_SYMBOL)
                            return items;

                        break;
                    }
                }
            }

            return items;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int ParseName( out int startIndex )
        {
            startIndex = _idx;
            switch(_textSpan[ _idx ])
            {
                case QUOTE_SYMBOL:

                    _idx++;
                    startIndex = _idx;
                    int endIndex = ParseQuotation( out bool escape, out bool unicode );

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
                    startIndex = _idx;
                    for(; true; _idx++)
                    {
                        int lastNameCharIndex = _idx;

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
                        var sp = new SimpleParam2( _text ) { ValueStartIndex = startIndex, ValueEndIndex = ParseQuotation( out bool escape, out bool unicode ) };
                        sp.ContainsEscapedChars = escape;
                        sp.ContainsLiteralUnicodeChars = unicode;
                        return sp;
                    }

                    default:
                    {
                        _idx++;
                        for(; _idx < _textSpan.Length; _idx++)
                        {
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

    public class AVX2Utils
    {

        private static readonly byte[] _valuesToAvoid = new byte[] { 9, 10, 13, 32 };
        private static readonly Vector256<byte>[] _scalarVectors = _valuesToAvoid.Select( Vector256.Create<byte> ).ToArray();

        //private static IEnumerable<char> RemoveWhitespace( byte[] data )
        //{
        //    int dataLength = data.Length;
        //    int vectorSize = Vector256<byte>.Count;
        //    int chunks = dataLength / vectorSize; //evito ultimo iterazione

        //    for(int chunkIndex = 0; chunkIndex < chunks; chunkIndex++)
        //    {
        //        var dataSpan = new ReadOnlySpan<byte>( data );
        //        int currentIndex = chunkIndex * vectorSize;

        //        Vector256<byte> dataVector = Vector256.Create( dataSpan.Slice( currentIndex, 32 ) );
        //        Vector256<byte> resultMask = Vector256<byte>.Zero;

        //        foreach(var scalarValue in _scalarVectors)
        //            resultMask = Avx2.Or( resultMask, Avx2.CompareEqual( dataVector, scalarValue ) );

        //        // Use permutevar8x32 to gather non-zero elements
        //        Vector256<byte> shuffledResult = Avx2.Permute2x128( dataVector, Vector256<byte>.AllBitsSet, 0b01 );


        //        var shuffled = Avx2.BlendVariable( dataVector, Vector256<byte>.Zero, resultMask );
        //        for(int i = 0; i < 32; i++)
        //            yield return (char)shuffled[ i ];
        //    };

        //    //handle remaining elements not multiple of vector size
        //    int procIndex = chunks * vectorSize;
        //    if(procIndex != dataLength - 1)
        //    {
        //        var dataSpan = new ReadOnlySpan<byte>( data );

        //        Vector256<byte> dataVector = Vector256<byte>.Zero;
        //        for(int i = 0; i < dataLength - procIndex; i++)
        //            dataVector = dataVector.WithElement( i, dataSpan[ i ] );

        //        Vector256<byte> resultMask = Vector256<byte>.Zero;

        //        foreach(var scalarValue in _scalarVectors)
        //            resultMask = Avx2.Or( resultMask, Avx2.CompareEqual( dataVector, scalarValue ) );

        //        var shuffled = Avx2.BlendVariable( dataVector, Vector256<byte>.Zero, resultMask );

        //        for(int i = 0; i < dataLength - procIndex; i++)
        //            yield return (char)shuffled[ i ];
        //    }
        //}

        private static int[] GetWhitespaceMask( ReadOnlySpan<byte> data )
        {
            int dataLength = data.Length;
            int vectorSize = Vector256<byte>.Count;
            int chunks = dataLength / vectorSize; //evito ultimo iterazione
            int rem = (dataLength % vectorSize == 0) ? 0 : 1;
            int totalMasks = chunks + rem;

            int[] masks = new int[ totalMasks ];
            for(int chunkIndex = 0; chunkIndex < chunks; chunkIndex++)
            {
                int currentIndex = chunkIndex * vectorSize;

                Vector256<byte> dataVector = Vector256.Create( data.Slice( currentIndex, vectorSize ) );
                Vector256<byte> resultMask = Vector256<byte>.Zero;

                foreach(var scalarValue in _scalarVectors)
                    resultMask = Avx2.Or( resultMask, Avx2.CompareEqual( dataVector, scalarValue ) );

                masks[chunkIndex]= Avx2.MoveMask( resultMask );
            };

            //handle remaining elements not multiple of vector size
            int procIndex = chunks * vectorSize;
            if(procIndex < dataLength)
            {
                Vector256<byte> dataVector = Vector256<byte>.Zero;
                for(int i = 0; i < dataLength - procIndex; i++)
                    dataVector = dataVector.WithElement( i, data[ procIndex + i ] );

                Vector256<byte> resultMask = Vector256<byte>.Zero;

                foreach(var scalarValue in _scalarVectors)
                    resultMask = Avx2.Or( resultMask, Avx2.CompareEqual( dataVector, scalarValue ) );

                masks[totalMasks-1] = Avx2.MoveMask( resultMask );
            }

            return masks;
        }

        public static string GetStringWithoutWhitespaces( string text )
        {
            int bits = 32;

            ReadOnlySpan<char> charSpan = text.AsSpan();

            // Convert to byte span using UTF-8 encoding
            Encoding utf8 = Encoding.UTF8;
            int byteCount = utf8.GetByteCount( charSpan );
            Span<byte> byteSpan = new byte[ byteCount ];
            utf8.GetBytes( charSpan, byteSpan );

            // Now you have the data as a ReadOnlySpan<byte>
            ReadOnlySpan<byte> readOnlyByteSpan = byteSpan;

            var sb = new StringBuilder();
            var masks = GetWhitespaceMask( readOnlyByteSpan );
            int m = 0;
            foreach(var mask in masks)
            {
                int offset = m * bits;
                for(int j = 0; j < bits; j++)
                {
                    if(j + offset >= byteCount) break;
                    if((mask & (1 << j)) == 0)
                        sb.Append( text[ j + offset ] );
                }
                m++;
            }
            return sb.ToString();
        }
    }
#endif
}
