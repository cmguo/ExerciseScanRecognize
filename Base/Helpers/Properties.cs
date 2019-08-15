using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Helpers
{
    public class Properties
    {

        public void Apply(object src, object dst, IDictionary<string, object> others)
        {
        }

        internal class PropertyCopier
        {

            internal static Expression CreatePropertyAssigner(Type src, Type dst)
            {
                ParameterExpression sourceParameter = Expression.Parameter(src, "src");
                ParameterExpression targetParameter = Expression.Parameter(dst, "dst");
                var expressions = new List<Expression>();
                foreach (PropertyInfo sourceProperty in src.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!sourceProperty.CanRead)
                        continue;
                    PropertyInfo targetProperty = dst.GetType().GetProperty(sourceProperty.Name);
                    if (targetProperty == null || !targetProperty.CanWrite
                        || (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0)
                    {
                        targetProperty = null;
                    }
                    bool assignable = targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType);
                    BinaryExpression targetNull = Expression.Equal(Expression.Property(targetParameter, targetProperty),
                        Expression.Default(targetProperty.DeclaringType));
                    if (assignable)
                    {
                        Expression assgin = Expression.Assign(
                            Expression.Property(targetParameter, targetProperty),
                            Expression.Property(sourceParameter, sourceProperty));
                        expressions.Add(Expression.IfThenElse(targetNull, assgin, 
                            CreatePropertyAssigner(sourceProperty.DeclaringType, targetProperty.DeclaringType)));
                    }
                    else
                    {

                    }
                }
                return Expression.Lambda(Expression.Block(expressions), targetParameter, sourceParameter);
            }
        }
    }
}
