using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators
{
    public class ProxyClassTypeGenerator : ProxyTypeGenerator
    {
        private Type[] _additionalConstrcutorParameterTypes;

        public ProxyClassTypeGenerator(Type baseType, Type implemetionType)
            : base(baseType, implemetionType)
        {
            _additionalConstrcutorParameterTypes = new[] { typeof(IInterceptorCollector), typeof(IMethodInvocationExecutor) };
        }

        #region Constructor

        protected override void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameters = consturctor.GetParameters().Select(i => i.ParameterType).ToArray();
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, _additionalConstrcutorParameterTypes.Concat(parameters).ToArray())
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = 2; i < parameters.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, i + 1);
            }

            ilGen.Emit(OpCodes.Call, consturctor);
            ilGen.Emit(OpCodes.Ret);
        }

        protected override void CreateTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, _additionalConstrcutorParameterTypes)
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ret);
        }

        protected override ConstructorInfo[] GetBaseConstructorInfos()
        {
            return ImplementionType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        #endregion  
    }
}
