using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tinja.Interception
{
    public interface IInterceptorBinding
    {
        int Priority { get; }

        bool Inherited { get; }

        Type Interceptor { get; }

        bool Supported(MemberInfo memberInfo);
    }

    public class TypeIntercetproBinding : IInterceptorBinding
    {
        public int Priority => throw new NotImplementedException();

        public bool Inherited => throw new NotImplementedException();

        public Type Interceptor => throw new NotImplementedException();

        public Type TargetType { get; set; }

        public bool Supported(MemberInfo memberInfo)
        {
            return TargetType == memberInfo.DeclaringType;
        }
    }

    public class MemberInterceptorBinding : IInterceptorBinding
    {
        public int Priority => throw new NotImplementedException();

        public bool Inherited => throw new NotImplementedException();

        public Type Interceptor => throw new NotImplementedException();

        public HashSet<MemberInfo> TargetMembers { get; }

        public bool Supported(MemberInfo memberInfo)
        {
            return TargetMembers.Contains(memberInfo);
        }
    }

    public class InterceptorBinding
    {
        public Type TargetType { get; private set; }

        public HashSet<MemberInfo> TargetMembers { get; }

        public InterceptorAttribute Interceptor { get; }

        public InterceptorBinding(InterceptorAttribute interceptor)
        {
            Interceptor = interceptor;
            TargetMembers = new HashSet<MemberInfo>();
        }

        public bool Supported(MemberInfo memberInfo)
        {
            return TargetMembers.Any(i => i == memberInfo);
        }

        public void AddTarget(MemberInfo memberInfo)
        {
            if (memberInfo is Type typeInfo)
            {
                if (TargetType == null)
                {
                    TargetType = typeInfo;
                }

                if (TargetType != typeInfo)
                {
                    throw new NotSupportedException("two different targets");
                }
            }
            else if (memberInfo != null)
            {
                TargetMembers.Add(memberInfo);
            }
        }
    }
}
