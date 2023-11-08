//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.PortableExecutable;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using UltraMapper.Parsing;

//namespace UltraMapper.Json.Parsers.other_parsers
//{
//    //USING microsoft reader
//    internal class Ut8JsonReaderAdapter : IParser
//    {
//        public IParsedParam Parse( string text )
//        {
//            var options = new JsonReaderOptions
//            {
//                AllowTrailingCommas = true,
//                CommentHandling = JsonCommentHandling.Skip
//            };

//            var bytes = StringEncoding.UTF8.GetBytes( text, 0, text.Length );
//            var reader = new Utf8JsonReader( bytes, options );

//            IParsedParam parsedParam = null;

//            while(reader.Read())
//            {
//                switch(reader.TokenType)
//                {
//                    case JsonTokenType.StartArray:
//                    {
//                        parsedParam = new ArrayParam() { Name = String.Empty };
//                        ReadArray( ref reader, (ArrayParam)parsedParam );
//                        return parsedParam;
//                    }
//                    case JsonTokenType.StartObject:
//                    {
//                        parsedParam = new ComplexParam() { Name = String.Empty, SubParams = new List<IParsedParam>( 0 ) };
//                        ReadObject( ref reader, (ComplexParam)parsedParam );
//                        return parsedParam;
//                    }
//                }
//            }

//            return parsedParam;
//        }

//        private IParsedParam ReadArray( ref Utf8JsonReader reader, ArrayParam parsedParam )
//        {
//            string propertyName = "";
//            while(reader.Read())
//            {
//                switch(reader.TokenType)
//                {

//                    case JsonTokenType.StartArray:
//                    {
//                        var arr = new ArrayParam() { Name = propertyName };
//                        parsedParam.Add(arr );
//                        ReadArray( ref reader, arr );
//                        break;
//                    }
//                    case JsonTokenType.EndArray: return parsedParam;
//                    case JsonTokenType.StartObject:
//                    {
//                        var cp = new ComplexParam() { Name = propertyName, SubParams = new List<IParsedParam>( 0 ) };
//                        parsedParam.Add( cp );
//                        ReadObject( ref reader, cp );
//                        break;
//                    }

//                    case JsonTokenType.PropertyName:
//                        propertyName = reader.GetString();
//                        break;
//                    case JsonTokenType.String: parsedParam.Add( new SimpleParam() { Name = propertyName, Value = reader.GetString() } ); break;
//                    case JsonTokenType.Number: parsedParam.Add( new SimpleParam() { Name = propertyName, Value = reader.GetInt32().ToString() } ); break;
//                    case JsonTokenType.True: parsedParam.Add( new BooleanParam() { Name = propertyName, Value = Boolean.TrueString } ); break;
//                    case JsonTokenType.False: parsedParam.Add( new BooleanParam() { Name = propertyName, Value = Boolean.FalseString } ); break;
//                    case JsonTokenType.Null: parsedParam.Add( new SimpleParam() { Name = propertyName, Value = null } ); break;
//                }
//            }

//            return parsedParam;
//        }

//        private IParsedParam ReadObject( ref Utf8JsonReader reader, ComplexParam parsedParam )
//        {
//            string propertyName = String.Empty;
//            while(reader.Read())
//            {
//                switch(reader.TokenType)
//                {
//                    case JsonTokenType.StartArray:
//                    {
//                        var arr = new ArrayParam() { Name = propertyName };
//                        parsedParam.SubParams.Add( arr );
//                        ReadArray( ref reader, arr );
//                        break;
//                    }
//                    case JsonTokenType.StartObject:
//                    {
//                        var cp = new ComplexParam() { Name = propertyName, SubParams = new List<IParsedParam>( 0 ) };
//                        parsedParam.SubParams.Add( cp );
//                        ReadObject( ref reader, cp );
//                        break;
//                    }
//                    case JsonTokenType.EndObject: return parsedParam;

//                    case JsonTokenType.PropertyName:
//                        propertyName = reader.GetString();
//                        break;
//                    case JsonTokenType.String: parsedParam.SubParams.Add( new SimpleParam() { Name = propertyName, Value = reader.GetString() } ); break;
//                    case JsonTokenType.Number: parsedParam.SubParams.Add( new SimpleParam() { Name = propertyName, Value = reader.GetInt32().ToString() } ); break;
//                    case JsonTokenType.True: parsedParam.SubParams.Add( new BooleanParam() { Name = propertyName, Value = Boolean.TrueString } ); break;
//                    case JsonTokenType.False: parsedParam.SubParams.Add( new BooleanParam() { Name = propertyName, Value = Boolean.FalseString } ); break;
//                    case JsonTokenType.Null: parsedParam.SubParams.Add( new SimpleParam() { Name = propertyName, Value = null } ); break;
//                }
//            }

//            return parsedParam;
//        }
//    }
//}
