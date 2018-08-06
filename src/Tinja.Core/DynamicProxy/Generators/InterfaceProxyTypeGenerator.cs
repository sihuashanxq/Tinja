using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;
using Tinja.Core.DynamicProxy.Generators.Extensions;

namespace Tinja.Core.DynamicProxy.Generators
{
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

            ilGen.SetThisField(GetField("__executor"), () => ilGen.LoadArgument(1));
            ilGen.SetThisField(GetField("__accessor"), () => ilGen.LoadArgument(2));

            ilGen.Return();
        }
    }
}
