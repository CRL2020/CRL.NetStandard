using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace CRL.Core
{
    public class DynamicMethodHelper
    {
        /// <summary>
        /// 编译Task<TResult>的调用委托
        /// </summary>
        /// <param name="taskType"></param>
        /// <returns></returns>
        public static Func<object, Task<object>> BuildContinueTaskInvoker(Type taskType)
        {
            var resultMethod = BuilderMethodInvoker(taskType.GetProperty("Result").GetGetMethod(true));
            return new Func<object, Task<object>>(obj =>
            {
                var task = (Task)obj;
                task.Start();
                return task.ContinueWith(t =>
                {
                    //异常？
                    var result = resultMethod(t, null);
                    return result;
                });
            });
        }

        //public static Delegate CreateDelegate(Type type, ConstructorInfo ctor)
        //{
        //    MethodInfo invoke = type.GetMethod("Invoke");
        //    ParameterInfo[] invokeParams = invoke.GetParameters();
        //    ParameterInfo[] methodParams = ctor.GetParameters();
        //    // 要求参数数量匹配。
        //    if (invokeParams.Length == methodParams.Length)
        //    {
        //        // 构造函数的参数列表。
        //        ParameterExpression[] paramList = GetParameters(invokeParams);
        //        // 构造调用参数列表。
        //        Expression[] paramExps = GetParameterExpressions(paramList, 0, methodParams, 0);
        //        if (paramExps != null)
        //        {
        //            Expression methodCall = Expression.New(ctor, paramExps);
        //            methodCall = GetReturn(methodCall, invoke.ReturnType);
        //            if (methodCall != null)
        //            {
        //                return Expression.Lambda(type, methodCall, paramList).Compile();
        //            }
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// 实例化对象 用EMIT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public static Func<object> BuildConstructorInvoker(ConstructorInfo constructor)
        {
            var dynamicMethod = BuildDynamicMethod(constructor);
            return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
        }
 
        public static DynamicMethod BuildDynamicMethod(ConstructorInfo constructor)
        {
            var _parameters = constructor.GetParameters();
            var _parameterTypes = _parameters?.Select(b => b.ParameterType).ToArray();
            var dynamicMethod = new DynamicMethod("", constructor.ReflectedType, _parameterTypes, true);

            var il = dynamicMethod.GetILGenerator();
            int i = 0;
            if (_parameterTypes != null)
            {
                for (i = 0; i < _parameterTypes.Length; i++)
                    il.Emit(OpCodes.Ldarg, i);
            }
            il.Emit(OpCodes.Newobj, constructor);

            il.Emit(OpCodes.Ret);
            return dynamicMethod;
        }

        /// <summary>
        /// 动态编译成委托
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static Func<object, object[], object> BuilderMethodInvoker(MethodInfo methodInfo)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var instanceExpr = methodInfo.IsStatic ? null : Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            List<Expression> parameterExpressions = new List<Expression>();
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                var arrItem = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var arrCase = Expression.Convert(arrItem, paramInfos[i].ParameterType);
                parameterExpressions.Add(arrCase);
            }
            var callExpr = Expression.Call(instanceExpr, methodInfo, parameterExpressions);

            if (methodInfo.ReturnType == typeof(void))
            {
                var action = Expression.Lambda<Action<object, object[]>>(callExpr,
                    instanceParameter, parametersParameter).Compile();
                return (instance, parameters) =>
                {
                    action(instance, parameters);
                    return null;
                };
            }
            else
            {
                UnaryExpression castCallExpr = Expression.Convert(callExpr, typeof(object));
                var fun = Expression.Lambda<Func<object, object[], object>>(castCallExpr, instanceParameter, parametersParameter).Compile();
                return fun;
            }
        }
    }
}
