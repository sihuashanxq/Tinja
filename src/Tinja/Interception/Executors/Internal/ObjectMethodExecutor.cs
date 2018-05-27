using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Reflection.Emit;
using Tinja.Extension;

namespace Tinja.Interception.Internal
{
    internal class ObjectMethodExecutor : IObjectMethodExecutor
    {
        protected Func<object, object[], Task<object>> MethodExecutor { get; }

        public MethodInfo MethodInfo { get; }

        public ObjectMethodExecutor(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            MethodExecutor = typeof(Task).IsAssignableFrom(methodInfo.ReturnType)
                ? CreateAsyncExecutor(MethodInfo)
                : CreateAsyncExecutorWrapper(MethodInfo);
        }

        public Task<object> ExecuteAsync(object instance, object[] paramterValues)
        {
            return MethodExecutor(instance, paramterValues);
        }

        private static Func<object, object[], Task<object>> CreateAsyncExecutorWrapper(MethodInfo methodInfo)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var invoker = CreateMethodInvoker(methodInfo);
            var invocation = Expression.Invoke(Expression.Constant(invoker), instance, parametersParameter);

            var executor = Expression.Lambda(invocation, instance, parametersParameter).Compile();

            return (obj, paramterValues) =>
            {
                try
                {
                    return Task.FromResult(((Func<object, object[], object>)executor)(obj, paramterValues));
                }
                catch (Exception e)
                {
                    throw;
                }
            };
        }

        private static Func<object, object[], Task<object>> CreateAsyncExecutor(MethodInfo methodInfo)
        {
            var instance = Expression.Parameter(typeof(object), "instance");
            var taskCompletionSourceType = typeof(TaskCompletionSource<object>);
            var tcs = Expression.Parameter(taskCompletionSourceType, "tcs");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var invoker = CreateMethodInvoker(methodInfo);
            var methodInvocation = Expression.Invoke(Expression.Constant(invoker), instance, parametersParameter);

            var isVoidMethod = methodInfo.ReturnType == typeof(Task);
            var awaiterType = isVoidMethod
                ? typeof(TaskAwaiter)
                : typeof(TaskAwaiter<>).MakeGenericType(methodInfo.ReturnType.GetGenericArguments().FirstOrDefault());

            var task = Expression.Variable(methodInfo.ReturnType, "task");
            var awaiter = Expression.Variable(awaiterType, "awaiter");
            var getAwaiter = Expression.Call(task, methodInfo.ReturnType.GetMethod("GetAwaiter"));
            var getException = Expression.MakeMemberAccess(task, methodInfo.ReturnType.GetProperty("Exception"));

            var lambdaBody = Expression.Block(
                new[] { task, awaiter },
                Expression.Assign(task, methodInvocation),
                Expression.Assign(awaiter, getAwaiter),
                Expression.Call(
                    awaiter,
                    awaiterType.GetMethod("OnCompleted"),
                    Expression.Lambda<Action>(
                         Expression.IfThenElse(
                            Expression.NotEqual(getException, Expression.Constant(null)),
                            Expression.Call(
                                tcs,
                                taskCompletionSourceType.GetMethod("SetResult"),
                                getException
                            ),
                            Expression.Call(
                                tcs,
                                taskCompletionSourceType.GetMethod("SetResult"),
                                isVoidMethod
                                    ? (Expression)Expression.Constant(null)
                                    : Expression.Call(awaiter, awaiterType.GetMethod("GetResult"))
                            )
                        )
                    )
                ),
                Expression.Label(Expression.Label())
            );

            var lambda = Expression.Lambda(lambdaBody, instance, parametersParameter, tcs);
            var executor = (Action<object, object[], TaskCompletionSource<object>>)lambda.Compile();

            return (obj, args) =>
            {
                var taskCompletionSource = new TaskCompletionSource<object>();

                executor(obj, args, taskCompletionSource);

                return taskCompletionSource.Task;
            };
        }

        private static Func<object, object[], object> CreateMethodInvoker(MethodInfo methodInfo)
        {
            var argIndex = 0;
            var dyMethod = new DynamicMethod(methodInfo.Name, typeof(object), new[] { typeof(object), typeof(object[]) });
            var ilGen = dyMethod.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);

            foreach (var item in methodInfo.GetParameters().Select(n => n.ParameterType))
            {
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldc_I4, argIndex++);
                ilGen.Box(item);
                ilGen.Emit(OpCodes.Ldelem_Ref);
            }

            ilGen.Emit(OpCodes.Call, methodInfo);
            ilGen.Emit(methodInfo.IsVoidMethod() ? OpCodes.Ldnull : OpCodes.Nop);
            ilGen.Emit(OpCodes.Ret);

            return (Func<object, object[], object>)dyMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }
    }
}
