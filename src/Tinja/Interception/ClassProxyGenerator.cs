using System;
using System.Reflection;

namespace Tinja.Interception
{
    public class ClassProxyGenerator : ProxyGeneratorBase
    {
        public ClassProxyGenerator(Type baseType, Type implemetionType)
            : base(baseType, implemetionType)
        {

        }

        protected override void CreateConstrcutors()
        {
            var constructorInfos = BaseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructorInfos == null || constructorInfos.Length == 0)
            {
                CreateConstructor(null);
                return;
            }

            foreach (var item in constructorInfos)
            {
                CreateConstructor(item);
            }
        }

        protected override void CreateProperties()
        {

        }
    }
}
