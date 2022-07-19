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
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return !source.EntryType.IsBuiltIn( true ) && source.EntryType.IsEnumerable() && target.EntryType == typeof( JsonString );
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            var context = (CollectionMapperContext)this.GetMapperContext( mapping );
            var mappingExpression = context.MapperConfiguration[ context.SourceCollectionElementType, typeof( JsonString ) ].MappingExpression;

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
