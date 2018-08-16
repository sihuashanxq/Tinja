using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
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
            var ilGen = TypeBuilder
                .DefineConstructor(consturctor.Attributes, consturctor.CallingConvention, ExtraConstrcutorParameterTypes.Concat(parameterTypes).ToArray())
                .SetCustomAttributes(consturctor)
                .DefineParameters(parameterInfos, parameterInfos.Length + ExtraConstrcutorParameterTypes.Length)
                .GetILGenerator();

            ilGen.SetThisField(GetField("__builder"), () => ilGen.LoadArgument(1));

            var args = new Action<int>[parameterTypes.Length];
            if (args.Length == 0)
            {
                ilGen.Base(consturctor, args);
                ilGen.Return();
                return;
            }

            for (var i = 0; i < parameterTypes.Length; i++)
            {
                args[i] = argIndex => ilGen.LoadArgument(argIndex + ExtraConstrcutorParameterTypes.Length);
            }

            ilGen.Base(consturctor, args);
            ilGen.Return();
        }
    }
}
