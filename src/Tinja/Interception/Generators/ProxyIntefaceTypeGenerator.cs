using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators
{
    public class ProxyIntefaceTypeGenerator : ProxyTypeGenerator
    {
        private Type[] _additionalConstrcutorParameterTypes;

        public ProxyIntefaceTypeGenerator(Type baseType, Type implementionType)
            : base(baseType, implementionType)
        {
            _additionalConstrcutorParameterTypes = new[] { ImplementionType, typeof(IInterceptorCollector), typeof(IMethodInvocationExecutor) };
        }

        protected override void CreateTypeFields()
        {
            CreateField("__target", ImplementionType, FieldAttributes.Private);
            base.CreateTypeFields();
        }

        protected override void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameters = consturctor.GetParameters().Select(i => i.ParameterType).ToArray();
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, _additionalConstrcutorParameterTypes.Concat(parameters).ToArray())
                .GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Stfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = 3; i < parameters.Length; i++)
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
            ilGen.Emit(OpCodes.Stfld, GetField("__target"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_2);
            ilGen.Emit(OpCodes.Ldtoken, BaseType);
            ilGen.Emit(OpCodes.Ldtoken, ImplementionType);
            ilGen.Emit(OpCodes.Call, typeof(IInterceptorCollector).GetMethod("Collect"));
            ilGen.Emit(OpCodes.Stsfld, GetField("__interceptors"));

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_3);
            ilGen.Emit(OpCodes.Stsfld, GetField("__executor"));

            ilGen.Emit(OpCodes.Ret);
        }
    }
}
