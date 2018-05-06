using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tinja.Interception.Internal
{
    internal class ObjectMethodExecutor : IObjectMethodExecutor
    {
        protected Func<object, object[], Task<object>> MethodExecutor { get; }

        public Type DeclareType => MethodInfo.DeclaringType;

        public MethodInfo MethodInfo { get; }

        public ObjectMethodExecutor(MethodInfo methodInfo)
        {
            MethodInfo = MethodInfo;

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
            var instanceParamter = Expression.Parameter(methodInfo.DeclaringType, "instance");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var parameters = new List<Expression>();
            var parameterInfos = methodInfo.GetParameters();
            var returnType = methodInfo.ReturnType;

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                var paramterValue = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var castedValue = Expression.Convert(paramterValue, item.ParameterType);

                parameters.Add(castedValue);
            }

            var methodCall = Expression.Convert(
                Expression.Call(
                    instanceParamter,
                    methodInfo,
                    parameters
                ),
                returnType == typeof(void) ? typeof(void) : typeof(object)
            );

            var executor = Expression.Lambda(methodCall, instanceParamter, parametersParameter).Compile();

            //Wrap return void
            return (instance, paramterValues) =>
            {
                try
                {
                    if (returnType != typeof(void))
                    {
                        return Task.FromResult(((Func<object, object[], object>)executor)(instance, paramterValues));
                    }
                    else
                    {
                        ((Action<object, object[]>)executor)(instance, paramterValues);
                        return Task.FromResult<object>(null);
                    }
                }
                catch (Exception e)
                {
                    return Task.FromResult<object>(e);
                }
            };
        }

        private static Func<object, object[], Task<object>> CreateAsyncExecutor(MethodInfo methodInfo)
        {
            var tcsType = typeof(TaskCompletionSource<object>);
            var returnType = methodInfo.ReturnType;

            var tcsParamter = Expression.Parameter(tcsType, "tcs");
            var instanceParamter = Expression.Parameter(methodInfo.DeclaringType, "service");
            var paramsParameter = Expression.Parameter(typeof(object[]), "parameters");

            var parameters = new List<Expression>();
            var parameterInfos = methodInfo.GetParameters();

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                var paramterValue = Expression.ArrayIndex(paramsParameter, Expression.Constant(i));
                var castedValue = Expression.Convert(paramterValue, item.ParameterType);

                parameters.Add(castedValue);
            }

            var methodCall = Expression.Call(
                  instanceParamter,
                  methodInfo,
                  parameters
            );

            var isVoidMethod = returnType == typeof(Task);
            var awaiterType = isVoidMethod
                ? typeof(TaskAwaiter)
                : typeof(TaskAwaiter<>).MakeGenericType(returnType.GetGenericArguments().FirstOrDefault());

            var task = Expression.Variable(returnType, "task");
            var awaiter = Expression.Variable(awaiterType, "awaiter");
            var getAwaiter = Expression.Call(task, returnType.GetMethod("GetAwaiter"));
            var getException = Expression.MakeMemberAccess(task, returnType.GetProperty("Exception"));

            var lambdaBody = Expression.Block(
                new[] { task, awaiter },
                Expression.Assign(task, methodCall),
                Expression.Assign(awaiter, getAwaiter),
                Expression.Call(
                    awaiter,
                    awaiterType.GetMethod("OnCompleted"),
                    Expression.Lambda<Action>(
                         Expression.IfThenElse(
                            Expression.NotEqual(getException, Expression.Constant(null)),
                            Expression.Call(
                                tcsParamter,
                                tcsType.GetMethod("SetResult"),
                                getException
                            ),
                            Expression.Call(
                                tcsParamter,
                                tcsType.GetMethod("SetResult"),
                                isVoidMethod
                                    ? (Expression)Expression.Constant(null)
                                    : Expression.Call(awaiter, awaiterType.GetMethod("GetResult"))
                            )
                        )
                    )
                ),
                Expression.Label(Expression.Label())
            );

            var lambda = Expression.Lambda(lambdaBody, instanceParamter, paramsParameter, tcsParamter);
            var executor = (Action<object, object[], TaskCompletionSource<object>>)lambda.Compile();

            return (obj, paramterValues) =>
            {
                var tcs = new TaskCompletionSource<object>();

                executor(obj, paramterValues, tcs);

                return tcs.Task;
            };
        }
    }
}
