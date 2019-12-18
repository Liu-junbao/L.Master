using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace System
{
    public static class Extensions
    {
        /// <summary>
        /// 查找指定类型的方法调用
        /// </summary>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="expression"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static IEnumerable<MethodCallExpression> FindMethodCallExpressions(this Expression expression, Type declaringType, string methodName)
        {
            if (expression == null)
                yield break;

            if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;
                if (methodExpression.Method.DeclaringType == declaringType && methodExpression.Method.Name == methodName)
                    yield return methodExpression;
                foreach (var argExp in methodExpression.Arguments)
                {
                    foreach (var item in argExp.FindMethodCallExpressions(declaringType, methodName))
                    {
                        yield return item;
                    }
                }
            }
            else if (expression is LambdaExpression)
            {
                foreach (var item in ((LambdaExpression)expression).Body.FindMethodCallExpressions(declaringType, methodName))
                {
                    yield return item;
                }
            }
            yield break;
        }

        /// <summary>
        /// 查找指定类型的属性调用
        /// </summary>
        /// <typeparam name="TDeclaringType"></typeparam>
        /// <param name="expression"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static IEnumerable<MemberExpression> FindMemberExpressions<TDeclaringType>(this Expression expression, string memberName)
        {
            if (expression == null)
                yield break;

            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                if (memberExpression.Member.DeclaringType == typeof(TDeclaringType) && memberExpression.Member.Name == memberName)
                    yield return memberExpression;
                foreach (var member in memberExpression.Expression.FindMemberExpressions<TDeclaringType>(memberName))
                {
                    yield return member;
                }
            }
            else if (expression is LambdaExpression)
            {
                foreach (var member in ((LambdaExpression)expression).Body.FindMemberExpressions<TDeclaringType>(memberName))
                {
                    yield return member;
                }
            }
            else if (expression is BinaryExpression)
            {
                var binaryExpression = (BinaryExpression)expression;
                foreach (var member in binaryExpression.Left.FindMemberExpressions<TDeclaringType>(memberName))
                {
                    yield return member;
                }
                foreach (var member in binaryExpression.Right.FindMemberExpressions<TDeclaringType>(memberName))
                {
                    yield return member;
                }
                foreach (var member in binaryExpression.Conversion.FindMemberExpressions<TDeclaringType>(memberName))
                {
                    yield return member;
                }
            }
            yield break;
        }

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
