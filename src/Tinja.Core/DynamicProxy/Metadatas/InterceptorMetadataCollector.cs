using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class InterceptorMetadataCollector : IInterceptorMetadataCollector
    {
        protected ConcurrentDictionary<MemberInfo, IEnumerable<InterceptorMetadata>> Cache { get; }

        public InterceptorMetadataCollector(IMemberMetadataProvider memberCollectorFactory)
        {
            Cache = new ConcurrentDictionary<MemberInfo, IEnumerable<InterceptorMetadata>>();
        }

        public IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata)
        {
            return Cache.GetOrAdd(metadata.Member, key => Collect(metadata.Member, metadata.InterfaceInherits.Select(item => item.Member).ToArray()));
        }

        protected virtual IEnumerable<InterceptorMetadata> Collect(MemberInfo memberInfo, MemberInfo[] implementsInterfaces)
        {
            switch (memberInfo)
            {
                case MethodInfo methodInfo:
                    return Collect(methodInfo, implementsInterfaces.OfType<MethodInfo>().ToArray());
                case PropertyInfo propertyInfo:
                    return Collect(propertyInfo, implementsInterfaces.OfType<PropertyInfo>().ToArray());
                default:
                    return new InterceptorMetadata[0];
            }
        }

        protected virtual IEnumerable<InterceptorMetadata> Collect(MethodInfo methodInfo, MethodInfo[] implementsInterfaces)
        {
            foreach (var attr in methodInfo.DeclaringType.GetInterceptorAttributes())
            {
                yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, methodInfo);
            }

            foreach (var attr in methodInfo.GetInterceptorAttributes())
            {
                yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, methodInfo);
            }

            foreach (var implementsInterface in implementsInterfaces)
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, methodInfo);
                }
            }

            foreach (var implementsInterface in implementsInterfaces.Select(item => item.DeclaringType))
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, methodInfo);
                }
            }
        }

        protected virtual IEnumerable<InterceptorMetadata> Collect(PropertyInfo propertyInfo, PropertyInfo[] implementsInterfaces)
        {
            foreach (var attr in propertyInfo.DeclaringType.GetInterceptorAttributes())
            {
                yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, propertyInfo);
            }

            foreach (var attr in propertyInfo.GetInterceptorAttributes())
            {
                yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, propertyInfo);
            }

            foreach (var implementsInterface in implementsInterfaces)
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, propertyInfo);
                }
            }

            foreach (var implementsInterface in implementsInterfaces.Select(item => item.DeclaringType))
            {
                foreach (var attr in implementsInterface.GetInterceptorAttributes())
                {
                    yield return new InterceptorMetadata(attr.Order, attr.InterceptorType, propertyInfo);
                }
            }
        }
    }
}
