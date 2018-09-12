using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
    [DisableProxy]
    public class ClassProxyTypeGenerator : ProxyTypeGenerator
    {
        public ClassProxyTypeGenerator(Type classType, IEnumerable<MemberMetadata> members)
            : base(classType, members)
        {

        }

        protected override void BuildTypeConstrcutors()
        {
            foreach (var item in TargetType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                CreateTypeConstructor(item);
            }

            BuildTypeStaticConstrcutor();
        }

        protected virtual void CreateTypeConstructor(ConstructorInfo consturctor)
        {
            var parameterInfos = consturctor.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var argumentStuffers = new Action<int>[parameterTypes.Length];
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, ExtraConstrcutorParameterTypes.Concat(parameterTypes).ToArray())
                .SetCustomAttributes(consturctor)
                .DefineParameters(parameterInfos, parameterInfos.Length + ExtraConstrcutorParameterTypes.Length)
                .GetILGenerator();

            ilGen.SetThisField(GetField("__builder"), () => ilGen.LoadArgument(1));

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                argumentStuffers[i] = argIndex => ilGen.LoadArgument(argIndex + ExtraConstrcutorParameterTypes.Length);
            }

            ilGen.Base(consturctor, argumentStuffers);
            ilGen.Return();
        }
    }
}
