using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Core.DynamicProxy.Registrations;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class ConfiguredInterceptorMetadataCollector : IInterceptorMetadataCollector
    {
        internal List<InterceptorTypeRegistration> Types { get; }

        internal List<InterceptorDelegateRegistration> Delegates { get; }

        public ConfiguredInterceptorMetadataCollector(IEnumerable<IInterceptorRegistration> registrations)
        {
            Types = new List<InterceptorTypeRegistration>();
            Delegates = new List<InterceptorDelegateRegistration>();

            AddRegistrations(registrations);
        }

        public IEnumerable<InterceptorMetadata> Collect(MemberMetadata metadata)
        {
            foreach (var registration in Types)
            {
                if (registration.TargetFilter == null || registration.TargetFilter(metadata.Member))
                {
                    yield return new InterceptorMetadata(registration.InterecptorType, metadata.Member, registration.RankOrder);
                }
            }

            foreach (var registration in Delegates)
            {
                if (registration.TargetFilter == null || registration.TargetFilter(metadata.Member))
                {
                    yield return new InterceptorMetadata(registration.Handler, metadata.Member, registration.RankOrder);
                }
            }
        }

        private void AddRegistrations(IEnumerable<IInterceptorRegistration> registrations)
        {
            foreach (var registration in registrations)
            {
                if (registration is InterceptorTypeRegistration typeRegistration)
                {
                    Types.Add(typeRegistration);
                }

                if (registration is InterceptorDelegateRegistration delegateRegistration)
                {
                    Delegates.Add(delegateRegistration);
                }
            }
        }
    }
}
