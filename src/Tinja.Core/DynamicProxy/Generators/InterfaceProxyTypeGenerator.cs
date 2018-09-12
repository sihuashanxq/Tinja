using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
    [DisableProxy]
    public class InterfaceProxyTypeGenerator : ProxyTypeGenerator
    {
        public InterfaceProxyTypeGenerator(Type interfaceType, IEnumerable<MemberMetadata> members)
            : base(interfaceType, members)
        {

        }

        protected override void BuildTypeConstrcutors()
        {
            BuildTypeStaticConstrcutor();
            BuildTypeDefaultConstructor();
        }

        protected virtual void BuildTypeDefaultConstructor()
        {
            var ilGen = TypeBuilder
                .DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, ExtraConstrcutorParameterTypes)
                .GetILGenerator();

            ilGen.SetThisField(GetField("__builder"), () => ilGen.LoadArgument(1));

            ilGen.Return();
        }
    }
}
