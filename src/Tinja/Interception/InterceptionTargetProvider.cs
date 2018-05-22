using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Tinja.Interception.TypeMembers;

namespace Tinja.Interception
{
    public class InterceptionTargetProvider : IInterceptionTargetProvider
    {
        private ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptionTarget>> _targetCaches;

        public InterceptionTargetProvider()
        {
            _targetCaches = new ConcurrentDictionary<Tuple<Type, Type>, IEnumerable<InterceptionTarget>>();
        }

        public IEnumerable<InterceptionTarget> GetTargets(Type baseType, Type implementionType)
        {
            return _targetCaches.GetOrAdd(Tuple.Create(baseType, implementionType), key =>
            {
                return CollectTargets(baseType, implementionType);
            });
        }

        protected IEnumerable<InterceptionTarget> CollectTargets(Type baseType, Type implementionType)
        {
            var targets = new Dictionary<Type, InterceptionTarget>();
            var typeMembers = TypeMemberCollector.Collect(baseType, implementionType);

            foreach (var typeMember in typeMembers.Where(i => !i.IsEvent))
            {
                CollectTargets(typeMember.Member, typeMember.InterfaceMembers, targets);
                CollectTargets(typeMember.DeclaringType, typeMember.Interfaces, targets);
            }

            return targets.Values;
        }

        protected virtual void CollectTargets(MemberInfo memberInfo, IEnumerable<MemberInfo> interfaceMembers, Dictionary<Type, InterceptionTarget> targets)
        {
            var attributes = interfaceMembers
                .SelectMany(i => i.GetInterceptorAttributes())
                .Where(i => i.Inherited)
                .Concat(memberInfo.GetInterceptorAttributes());

            foreach (var attribute in attributes)
            {
                var target = targets.GetValueOrDefault(attribute.InterceptorType);
                var memberPriority = new InterceptionMemberPriority()
                {
                    Priority = attribute.Priority,
                    MemberInfo = memberInfo
                };

                if (target == null)
                {
                    target = targets[attribute.InterceptorType] = new InterceptionTarget()
                    {
                        InterceptorType = attribute.InterceptorType,
                        Members = new HashSet<InterceptionMemberPriority>()
                    };
                }

                target.Members.Add(new InterceptionMemberPriority() { Priority = attribute.Priority, MemberInfo = memberInfo });
            }
        }
    }
}
