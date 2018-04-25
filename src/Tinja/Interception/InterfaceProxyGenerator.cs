using System;

namespace Tinja.Interception
{
    public class InterfaceProxyGenerator : ProxyGeneratorBase
    {
        public InterfaceProxyGenerator(Type baseType, Type implemetionType)
            : base(baseType, implemetionType)
        {

        }

        protected override void CreateConstrcutors()
        {
            CreateConstructor(null);
        }

        protected override void CreateProperties()
        {

        }
    }
}
