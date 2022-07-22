using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.Json.UltraMapper.Extensions;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.Json
{
    public sealed class JsonSerializer<T>
        where T : class, new()
    {
        private readonly ReferenceTracker _referenceTracker = new ReferenceTracker();
        private readonly JsonString _jsonString = new JsonString();
        private readonly IParser Parser = new JsonParser();

        public CultureInfo Culture { get; set; }
            = CultureInfo.InvariantCulture;

        public static Mapper Mapper = new Mapper( cfg =>
        {
            cfg.IsReferenceTrackingEnabled = false;
            cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;

            cfg.Conventions.GetOrAdd<DefaultConvention>( rule =>
            {
                rule.SourceMemberProvider.IgnoreFields = true;
                rule.SourceMemberProvider.IgnoreMethods = true;
                rule.SourceMemberProvider.IgnoreNonPublicMembers = true;

                rule.TargetMemberProvider.IgnoreFields = true;
                rule.TargetMemberProvider.IgnoreMethods = true;
                rule.TargetMemberProvider.IgnoreNonPublicMembers = true;
            } );

            cfg.Mappers.AddBefore<ReferenceMapper>( new IMappingExpressionBuilder[]
            {
                new ArrayParamExpressionBuilder(),
                new ComplexParamExpressionBuilder(){ CanMapByIndex = false },
                new SimpleParamExpressionBuilder(),
                new ObjectToJsonMapper(),
                new EnumerableToJsonMapper()
            } );
        } );

        private readonly Action<ReferenceTracker, object, object> _desMap;
        private readonly Action<ReferenceTracker, object, object> _serMap;

        public JsonSerializer()
        {
            Mapper.Config.MapTypes<string, DateTime>(
                s => DateTime.Parse( s, Culture ) );

            Mapper.Config.MapTypes<BooleanParam, bool>( s => s.BoolValue );

            _desMap = Mapper.Config[ typeof( ComplexParam ), typeof( T ) ].MappingFunc;
            _serMap = Mapper.Config[ typeof( T ), typeof( JsonString ) ].MappingFunc;
        }

        public JsonSerializer( IParser parser )
            : this()
        {
            Parser = parser;
        }

        public T Deserialize( string str )
        {
            return this.Deserialize( str, new T() );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T Deserialize( string str, T instance )
        {
            var parsedContent = this.Parser.Parse( str );
            _desMap( _referenceTracker, parsedContent, instance );
            return instance;
        }

        public string Serialize( T instance )
        {
            _jsonString.Json.Clear();
            _referenceTracker.Clear();

            _serMap( _referenceTracker, instance, _jsonString );
            return _jsonString.Json.ToString();
        }
    }

    public sealed class JsonSerializer
    {
        private readonly ReferenceTracker _referenceTracker = new ReferenceTracker();
        private readonly JsonString _jsonString = new JsonString();
        private readonly IParser Parser = new JsonParser();

        public CultureInfo Culture { get; set; }
            = CultureInfo.InvariantCulture;

        public static Mapper Mapper = new Mapper( cfg =>
        {
            cfg.IsReferenceTrackingEnabled = false;
            cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;

            cfg.Conventions.GetOrAdd<DefaultConvention>( rule =>
            {
                rule.SourceMemberProvider.IgnoreFields = true;
                rule.SourceMemberProvider.IgnoreMethods = true;
                rule.SourceMemberProvider.IgnoreNonPublicMembers = true;

                rule.TargetMemberProvider.IgnoreFields = true;
                rule.TargetMemberProvider.IgnoreMethods = true;
                rule.TargetMemberProvider.IgnoreNonPublicMembers = true;
            } );

            cfg.Mappers.AddBefore<ReferenceMapper>( new IMappingExpressionBuilder[]
            {
                new ArrayParamExpressionBuilder(),
                new ComplexParamExpressionBuilder(){ CanMapByIndex = false },
                new SimpleParamExpressionBuilder(),
                new ObjectToJsonMapper(),
                new EnumerableToJsonMapper()
            } );
        } );

        private Type lastMapType = null;
        private Action<ReferenceTracker, object, object> _map;

        public JsonSerializer()
        {
            Mapper.Config.MapTypes<string, DateTime>(
                s => DateTime.Parse( s, Culture ) );

            Mapper.Config.MapTypes<BooleanParam, bool>( s => s.BoolValue );
        }

        public JsonSerializer( IParser parser )
            : this()
        {
            Parser = parser;
        }

        //public T Deserialize<T>( string str ) where T : class, new()
        //{
        //    var parsedJson = this.Parser.Parse( str );
        //    return this.DeserializeInternal( parsedJson, new T() );
        //}

        public T Deserialize<T>( string str )
        {
            var parsedContent = this.Parser.Parse( str );

            T instance;

            if( typeof( T ).IsArray )
            {
                var arrayLength = ((ArrayParam)parsedContent).Items.Count;
                instance = InstanceFactory.CreateObject<int, T>( arrayLength );
            }
            else instance = InstanceFactory.CreateObject<T>();

            return this.DeserializeInternal( parsedContent, instance );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T Deserialize<T>( string str, T instance )
        {
            var parsedJson = this.Parser.Parse( str );
            return DeserializeInternal( parsedJson, instance );
        }

        private T DeserializeInternal<T>( IParsedParam parsedJson, T instance )
        {
            if( lastMapType != typeof( T ) )
            {
                lastMapType = typeof( T );

                if( typeof( T ).IsEnumerable() && !typeof( T ).IsBuiltIn( true ) )
                    _map = Mapper.Config[ typeof( ArrayParam ), typeof( T ) ].MappingFunc;
                else
                    _map = Mapper.Config[ typeof( ComplexParam ), typeof( T ) ].MappingFunc;
            }

            _map( _referenceTracker, parsedJson, instance );
            return instance;
        }

        public string Serialize<T>( T instance )
        {
            _jsonString.Json.Clear();
            _referenceTracker.Clear();

            var map = Mapper.Config[ typeof( T ), typeof( JsonString ) ].MappingFunc;
            map( _referenceTracker, instance, _jsonString );
            return _jsonString.Json.ToString();
        }
    }
}
