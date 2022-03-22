﻿using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UltraMapper.Conventions;
using UltraMapper.Json.UltraMapper.Extensions;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.Json
{
    public sealed class JsonSerializer
    {
        private readonly JsonString _jsonString = new JsonString();
#if NET5_0_OR_GREATER
        private readonly IParser Parser = new JsonParserWithReadonlySpan();
#else
        private readonly IParser Parser = new JsonParserWithSubstrings();
#endif

        public CultureInfo Culture { get; set; }
            = CultureInfo.InvariantCulture;

        public Mapper Mapper = new Mapper( cfg =>
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
                new ArrayParamExpressionBuilder( cfg ),
                new ComplexParamExpressionBuilder( cfg ){ CanMapByIndex = false },
                new SimpleParamExpressionBuilder( cfg ),
                new ObjectToJsonMapper( cfg )
            } );
        } );

        private Type lastMapType = null;
        private Action<ReferenceTracker, object, object> _map = null;

        public JsonSerializer()
        {
            this.Mapper.Config.MapTypes<string, DateTime>(
                s => DateTime.Parse( s, Culture ) );
        }

        public JsonSerializer( IParser parser )
            : this()
        {
            Parser = parser;
        }

        public T Deserialize<T>( string str ) where T : class, new()
        {
            return this.Deserialize( str, new T() );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T Deserialize<T>( string str, T instance ) where T : class
        {
            var parsedContent = this.Parser.Parse( str );

            if( lastMapType != typeof( T ) )
            {
                lastMapType = typeof( T );
                _map = this.Mapper.Config[ typeof( ComplexParam ), typeof( T ) ].MappingFunc;
            }

            _map( null, parsedContent, instance );
            return instance;
        }

        public string Serialize<T>( T instance )
        {
            _jsonString.Json.Clear();

            var map = this.Mapper.Config[ typeof( T ), typeof( JsonString ) ].MappingFunc;
            map( null, instance, _jsonString );
            return _jsonString.Json.ToString();
        }
    }
}
