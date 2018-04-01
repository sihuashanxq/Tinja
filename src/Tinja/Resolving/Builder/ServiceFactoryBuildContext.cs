using System.Reflection;
using Tinja.Resolving.Descriptor;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving.Builder
{
    public class ServiceFactoryBuildContext
    {
        public ConstructorDescriptor Constructor { get; set; }

        public IResolvingContext ResolvingContext { get; set; }

        public ServiceFactoryBuildParamterContext[] ParamtersContext { get; set; }
    }

    public class ServiceFactoryBuildParamterContext
    {
        public ParameterInfo Parameter { get; set; }

        public ServiceFactoryBuildContext ParameterTypeContext { get; set; }
    }
}
