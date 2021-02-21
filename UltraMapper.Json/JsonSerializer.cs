using System;
using System.Reflection;
using System.Text;
using UltraMapper.Conventions;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing;
using UltraMapper.Parsing.Extensions;

namespace UltraMapper.Json
{
    public class JsonSerializer
    {
        public Mapper Mapper = new Mapper( cfg =>
        {
            cfg.IsReferenceTrackingEnabled = false;
            cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;

            cfg.Conventions.GetOrAdd<DefaultConvention>( rule =>
            {
                rule.TargetMemberProvider.IgnoreMethods = true;
                rule.TargetMemberProvider.IgnoreNonPublicMembers = true;
            } );
        } );

        public JsonParser Parser = new JsonParser();

        StringBuilder sb = new StringBuilder();

        public JsonSerializer()
        {
            int index = Mapper.MappingConfiguration.Mappers.FindIndex( m => m is ReferenceMapper );

            Mapper.MappingConfiguration.Mappers.InsertRange( index, new IMappingExpressionBuilder[]
            {
                new ArrayParamExpressionBuilder( Mapper.MappingConfiguration ),
                new ComplexParamExpressionBuilder( Mapper.MappingConfiguration ){ CanMapByIndex = false },
                new SimpleParamExpressionBuilder( Mapper.MappingConfiguration )
            } );
        }

        int Convert( int o )
        {
            sb.AppendLine( o.ToString() );
            return o;
        }

        string Convert( string o )
        {
            sb.AppendLine( o.ToString() );
            return o;
        }

        public string Serialize( object obj )
        {
            Mapper.MappingConfiguration.MapTypes<object, object>( o => sb.AppendLine( o.ToString() ) );
            Mapper.MappingConfiguration.MapTypes<int, int>( o => Convert( o ) );
            Mapper.MappingConfiguration.MapTypes<string, string>( o => Convert( o ) );

            var duplicate = Mapper.Map( obj );

            return sb.ToString();
        }

        public T Deserialize<T>( string str ) where T : class, new()
        {
            return this.Deserialize( str, new T() );
        }

        private Type instanceType = null;
        private Action<ReferenceTracker, object, object> _map = null;

        public T Deserialize<T>( string str, T instance ) where T : class
        {
            var parsedContent = this.Parser.Parse( str );

            if( instanceType != typeof( T ) )
            {
                instanceType = typeof( T );
                _map = this.Mapper.MappingConfiguration[ typeof( ComplexParam ),
                        typeof( T ) ].MappingFunc;
            }

            _map( null, parsedContent, instance );
          
            return instance;
        }
    }

    public interface ISerializationFormat
    {
        string MemberFormat( MemberInfo mi, object value );
    }

    //public class JsonSerializer : ISerializationFormat
    //{
    //    public bool QuotePropertyNames { get; set; }

    //    public string MemberFormat( MemberInfo mi, object value )
    //    {
    //        if( mi.ReflectedType == typeof( string ) )
    //            return $"\"{mi.Name}\":\"{value}\"";

    //        return $"\"{mi.Name}\":{value}";
    //    }
    //}
}
