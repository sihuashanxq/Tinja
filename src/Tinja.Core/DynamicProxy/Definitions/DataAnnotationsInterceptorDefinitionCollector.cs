using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Configuration;
using Tinja.Abstractions.DynamicProxy.Definitions;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Injection.Extensions;

namespace Tinja.Core.DynamicProxy.Definitions
{
    public class DataAnnotationsInterceptorDefinitionCollector : IInterceptorDefinitionCollector
    {
        protected InterceptionConfiguration Configuration { get; }

        protected ConcurrentDictionary<MemberInfo, IEnumerable<InterceptorDefinition>> Caches { get; }

        public DataAnnotationsInterceptorDefinitionCollector(InterceptionConfiguration configuration, IMemberMetadataProvider memberCollectorFactory)
        {
            Configuration = configuration;
            Caches = new ConcurrentDictionary<MemberInfo, IEnumerable<InterceptorDefinition>>();
        }

        public IEnumerable<InterceptorDefinition> Collect(MemberMetadata metadata)
        {
            if (!Configuration.EnableInterception)
            {
                return new InterceptorDefinition[0];
            }

            return Caches.GetOrAdd(metadata.Member, key => Collect(metadata.Member, metadata.InterfaceInherits.Select(item => item.Member).ToArray()));
        }

        protected virtual IEnumerable<InterceptorDefinition> Collect(MemberInfo memberInfo, MemberInfo[] implementsInterfaces)
        {
            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    return Collect(methodInfo, implementsInterfaces.OfType<MethodInfo>().ToArray());
                case PropertyInfo propertyInfo:
                    return Collect(propertyInfo, implementsInterfaces.OfType<PropertyInfo>().ToArray());
                default:
                    return new InterceptorDefinition[0];
            }
        }

        protected virtual IEnumerable<InterceptorDefinition> Collect(MethodInfo methodInfo, MethodInfo[] implementsInterfaces)
        {
            foreach (var attr in methodInfo.DeclaringType.GetInterceptorAttributes())
            {
                yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, methodInfo);
            }

            foreach (var attr in methodInfo.GetInterceptorAttributes())
            {
                yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, methodInfo);
            }

            foreach (var implementsInterface in implementsInterfaces)
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, methodInfo);
                }
            }

            foreach (var implementsInterface in implementsInterfaces.Select(item => item.DeclaringType))
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, methodInfo);
                }
            }
        }

        protected virtual IEnumerable<InterceptorDefinition> Collect(PropertyInfo propertyInfo, PropertyInfo[] implementsInterfaces)
        {
            foreach (var attr in propertyInfo.DeclaringType.GetInterceptorAttributes())
            {
                yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, propertyInfo);
            }

            foreach (var attr in propertyInfo.GetInterceptorAttributes())
            {
                yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, propertyInfo);
            }

            foreach (var implementsInterface in implementsInterfaces)
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, propertyInfo);
                }
            }

            foreach (var implementsInterface in implementsInterfaces.Select(item => item.DeclaringType))
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorDefinition(attr.Order, attr.InterceptorType, propertyInfo);
                }
            }
        }
    }
}
