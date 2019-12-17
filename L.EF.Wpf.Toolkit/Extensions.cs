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
        public static IEnumerable<MethodCallExpression> FindMethodCallExpressions(this Expression expression, Type type, string methodName)
        {
            if (expression is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)expression;
                if (methodExpression.Method.DeclaringType == type && methodExpression.Method.Name == methodName)
                    yield return methodExpression;
                foreach (var argExp in methodExpression.Arguments)
                {
                    foreach (var item in argExp.FindMethodCallExpressions(type, methodName))
                    {
                        yield return item;
                    }
                }
            }
            else if (expression is LambdaExpression)
            {
                foreach (var item in ((LambdaExpression)expression).Body.FindMethodCallExpressions(type, methodName))
                {
                    yield return item;
                }
            }
            yield break;
        }

        public static IEnumerable<MemberExpression> FindMemberExpressions<IType>(this Expression expression,string memberName)
        {
            //if (expression is MemberExpression)
            //{
            //    var memberExpression = (MemberExpression)expression;
            //    if (memberExpression.Type == type && memberExpression.Method.Name == methodName)
            //        yield return memberExpression;
            //    foreach (var argExp in memberExpression.Arguments)
            //    {
            //        foreach (var item in argExp.FindMethodCallExpressions(type, methodName))
            //        {
            //            yield return item;
            //        }
            //    }
            //}
            //else if (expression is LambdaExpression)
            //{
            //    foreach (var item in ((LambdaExpression)expression).Body.FindMethodCallExpressions(type, methodName))
            //    {
            //        yield return item;
            //    }
            //}
            yield break;
        }
    }
}
