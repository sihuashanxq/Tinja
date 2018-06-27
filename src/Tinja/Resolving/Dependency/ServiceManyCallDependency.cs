using System.Collections.Generic;
using System.Reflection;
using Tinja.Resolving.Context;
using Tinja.Resolving.Metadata;

namespace Tinja.Resolving.Dependency
{
    public class ServiceManyCallDependency : ServiceCallDependency
    {
        public ServiceCallDependency[] Elements { get; set; }

        public ServiceManyCallDependency(ServiceContext ctx, TypeConstructor constructor, ServiceCallDependency[] elements, Dictionary<ParameterInfo, ServiceCallDependency> parameters = null) : base(ctx, constructor, parameters)
        {
            Elements = elements;
        }

        public override bool ContainsPropertyCircular()
        {
            foreach (var item in Elements)
            {
                if (item.ContainsPropertyCircular())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
