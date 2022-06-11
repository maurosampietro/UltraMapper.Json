using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Json.UltraMapper.Extensions
{
    internal class EnumerableToJsonMapper : CollectionMapper
    {
        public EnumerableToJsonMapper( Configuration mappingConfiguration )
            : base( mappingConfiguration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return !source.IsBuiltIn( true ) && source.IsEnumerable() && target == typeof( JsonString );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = (CollectionMapperContext)this.GetMapperContext( source, target, options );
            var mappingExpression = MapperConfiguration[ context.SourceCollectionElementType, typeof( JsonString ) ].MappingExpression;

            var body = ExpressionLoops.ForEach( context.SourceInstance, context.SourceCollectionLoopingVar,
                Expression.Invoke( mappingExpression, context.ReferenceTracker,
                    context.SourceCollectionLoopingVar, context.TargetInstance ) );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
