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
        public static Func<object, Task<T>> BuildContinueTaskInvoker<T>(Type taskType)
        {
            var resultMethod = GetMethodInvoker(taskType.GetProperty("Result").GetGetMethod(true));
            return new Func<object, Task<T>>(obj =>
            {
                var task = (Task)obj;
                return task.ContinueWith(t =>
                {
                    var result = (T)resultMethod(t, null);
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
        /// 动态编译成委托,不能ref参数
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        static Func<object, object[], object> __BuilderMethodInvoker(MethodInfo methodInfo)
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

        #region emit
        public static Func<object, object[], object> GetMethodInvoker(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new Type[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            var invoder = (Func<object, object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>));
            return invoder;
        }

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
        #endregion

        /// <summary>
        /// 创建类型的构造器调用委托
        /// </summary>
        /// <typeparam name="TFunc">构造器调用委托</typeparam>
        /// <param name="type">类型</param>
        /// <param name="args">参数类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static TFunc CreateCtorFunc<TFunc>(Type type, Type[] args)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var ctor = type.GetConstructor(args);
            if (ctor == null)
            {
                var argTypeNames = string.Join(", ", args.Select(a => a.Name));
                throw new ArgumentException($"类型{type}不存在构造函数.ctor({argTypeNames})");
            }

            var parameters = args.Select(t => Expression.Parameter(t)).ToArray();
            var body = Expression.New(ctor, parameters);
            return Expression.Lambda<TFunc>(body, parameters).Compile();
        }
    }
}
