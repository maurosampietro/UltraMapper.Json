using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.Json.UltraMapper.Extensions
{
    internal class ObjectToJsonMapper : ReferenceMapper
    {
        private readonly SourceMemberProvider _sourceMemberProvider = new SourceMemberProvider()
        {
            IgnoreFields = true,
            IgnoreMethods = true,
            IgnoreNonPublicMembers = true,
        };

        public ObjectToJsonMapper( Configuration mappingConfiguration )
            : base( mappingConfiguration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return !source.EntryType.IsEnumerable() && target.EntryType == typeof( JsonString );
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            var context = this.GetMapperContext( mapping );
            var sourceMembers = this.SelectSourceMembers( source.EntryType ).OfType<PropertyInfo>().ToArray();

            var indentationParam = Expression.PropertyOrField( context.TargetInstance, nameof( JsonString.Indentation ) );

            var expressions = GetTargetStrings( sourceMembers, context, indentationParam );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

                Expression.Invoke( _appendLine, context.TargetInstance, Expression.Constant( "{" + Environment.NewLine ) ),
                Expression.PostIncrementAssign( indentationParam ),
                Expression.Block( expressions ),
                Expression.PostDecrementAssign( indentationParam ),
                Expression.Invoke( _appendLine, context.TargetInstance, Expression.Constant( "}" ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        readonly Expression<Action<JsonString, string>> _appendText = ( sb, text ) => AppendText( sb, text );
        private static void AppendText( JsonString sb, string text )
        {
            sb.Json.Append( text );
        }

        readonly Expression<Action<JsonString, string>> _appendLine = ( sb, text ) => AppendLine( sb, text );
        private static void AppendLine( JsonString sb, string text )
        {
            sb.Json.Append( sb.IndentationString );
            sb.Json.Append( text );
        }

        readonly Expression<Action<JsonString, string, string>> _appendMemberNameValue =
            ( sb, memberName, memberValue ) => AppendMemberNameValue( sb, memberName, memberValue );

        private static void AppendMemberNameValue( JsonString sb, string memberName, string memberValue )
        {
            sb.Json.Append( sb.IndentationString );
            sb.Json.Append( memberName ).Append( " : " );
            sb.Json.Append( memberValue ).Append( "," );
            sb.Json.AppendLine();
        }

        readonly Expression<Action<JsonString, string>> _appendMemberName =
            ( sb, memberName ) => AppendMemberName( sb, memberName );

        private static void AppendMemberName( JsonString sb, string memberName )
        {
            sb.Json.Append( sb.IndentationString );
            sb.Json.Append( memberName ).Append( " :" );
            sb.Json.AppendLine();
        }

        private IEnumerable<Expression> GetTargetStrings( PropertyInfo[] targetMembers,
            ReferenceMapperContext context,
            MemberExpression indentationParam )
        {
            for( int i = 0; i < targetMembers.Length; i++ )
            {
                var item = targetMembers[ i ];

                //It is important to check array/collections after built-in types
                //(ie: string implements IEnumerable<char>)

                if( item.PropertyType.IsBuiltIn( true ) )
                {
                    var memberAccess = Expression.Property( context.SourceInstance, item );
                    LambdaExpression toStringExp = MapperConfiguration[ item.PropertyType, typeof( string ) ].MappingExpression;

                    yield return Expression.Invoke( _appendMemberNameValue, context.TargetInstance,
                        Expression.Constant( item.Name ),
                        Expression.Invoke( toStringExp, memberAccess ) );
                }
                else if( item.PropertyType.IsEnumerable() )
                {
                    var memberAccess = Expression.Property( context.SourceInstance, item );

                    LambdaExpression toStringExp = MapperConfiguration[ item.PropertyType, typeof( JsonString ) ].MappingExpression;

                    yield return Expression.Invoke( _appendMemberName, context.TargetInstance, Expression.Constant( item.Name ) );
                    yield return Expression.Invoke( _appendLine, context.TargetInstance, Expression.Constant( "[" + Environment.NewLine ) );
                    yield return Expression.PostIncrementAssign( indentationParam );
                    yield return Expression.Invoke( toStringExp, context.ReferenceTracker, memberAccess, context.TargetInstance );
                    yield return Expression.Invoke( _appendLine, context.TargetInstance, Expression.Constant( Environment.NewLine + "]" ) );
                    yield return Expression.PostDecrementAssign( indentationParam );
                }
                else
                {
                    var trackedReference = Expression.Parameter( typeof( JsonString ), "trackedReference" );

                    var memberAccess = Expression.Property( context.SourceInstance, item );
                    var memberAccessParam = Expression.Parameter( item.PropertyType, "ma" );

                    yield return Expression.Invoke( _appendMemberName, context.TargetInstance, Expression.Constant( item.Name ) );

                    yield return Expression.Block
                    (
                        new[] { memberAccessParam, trackedReference },

                        Expression.Assign( memberAccessParam, memberAccess ),

                        ReferenceTrackingExpression.GetMappingExpression(
                            context.ReferenceTracker, memberAccessParam,
                            context.TargetInstance, Expression.Empty(),
                            context.Mapper, _mapper,
                            Expression.Constant( null, typeof( IMapping ) ) ).ReplaceParameter(trackedReference,"trackedReference")
                    );

                    if( i != targetMembers.Length - 1 )
                        yield return Expression.Invoke( _appendText, context.TargetInstance, Expression.Constant( "," + Environment.NewLine ) );
                    else
                        yield return Expression.Invoke( _appendText, context.TargetInstance, Expression.Constant( Environment.NewLine, typeof( string ) ) );
                }
            }
        }

        protected MemberInfo[] SelectSourceMembers( Type sourceType )
        {
            return _sourceMemberProvider.GetMembers( sourceType )
                .Select( ( m, index ) => new
                {
                    Member = m,
                    Options = m.GetCustomAttribute<OutOptionsAttribute>() ??
                            new OutOptionsAttribute() {/*Order = index*/ }
                } )
                .Where( m => !m.Options.IsIgnored )
                .OrderBy( info => info.Options.Order )
                .Select( m => m.Member )
                .ToArray();
        }
    }
}
