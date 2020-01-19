using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace System
{
    public static class Extensions
    {

        /// <summary>
        /// 查找类型的所有属性调用
        /// </summary>
        /// <typeparam name="TDeclaringType"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IEnumerable<MemberExpression> FindMemberExpressions<TDeclaringType>(this Expression expression)
        {
            if (expression == null)
                yield break;

            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                if (memberExpression.Member.DeclaringType == typeof(TDeclaringType))
                    yield return memberExpression;
                foreach (var member in memberExpression.Expression.FindMemberExpressions<TDeclaringType>())
                {
                    yield return member;
                }
            }
            else if (expression is LambdaExpression)
            {
                foreach (var member in ((LambdaExpression)expression).Body.FindMemberExpressions<TDeclaringType>())
                {
                    yield return member;
                }
            }
            else if (expression is BinaryExpression)
            {
                var binaryExpression = (BinaryExpression)expression;
                foreach (var member in binaryExpression.Left.FindMemberExpressions<TDeclaringType>())
                {
                    yield return member;
                }
                foreach (var member in binaryExpression.Right.FindMemberExpressions<TDeclaringType>())
                {
                    yield return member;
                }
                foreach (var member in binaryExpression.Conversion.FindMemberExpressions<TDeclaringType>())
                {
                    yield return member;
                }
            }
            yield break;
        }
    }
}
