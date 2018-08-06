using System;
using System.Collections.Generic;
using System.Linq;
using Tinja.Abstractions.DynamicProxy.Metadatas;
using Tinja.Abstractions.Extensions;

namespace Tinja.Core.DynamicProxy.Metadatas
{
    public class ClassMemberMetadataCollector : MemberMetadataCollector
    {
        /// <inheritdoc />
        protected override IEnumerable<MemberMetadata> CollectMethods(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetMethods(BindingFlag)
                .Where(m => m.IsOverrideable())
                .Select(methodInfo => CreateMemberMetadata(methodInfo, interfaces));
        }

        /// <inheritdoc />
        protected override IEnumerable<MemberMetadata> CollectProperties(Type typeInfo, Type[] interfaces)
        {
            return typeInfo
                .GetProperties(BindingFlag)
                .Where(m => m.IsOverrideable())
                .Select(property => CreateMemberMetadata(property, interfaces));
        }

        protected override IEnumerable<MemberMetadata> CollectEvents(Type typeInfo, Type[] interfaces)
        {
            foreach (var eventInfo in typeInfo.GetEvents(BindingFlag))
            {
                if (eventInfo.AddMethod != null && eventInfo.AddMethod.IsOverrideable())
                {
                    yield return CreateMemberMetadata(eventInfo, interfaces);
                    continue;
                }

                if (eventInfo.RemoveMethod != null && eventInfo.RemoveMethod.IsOverrideable())
                {
                    yield return CreateMemberMetadata(eventInfo, interfaces);
                    continue;
                }

                if (eventInfo.RaiseMethod != null && eventInfo.RaiseMethod.IsOverrideable())
                {
                    yield return CreateMemberMetadata(eventInfo, interfaces);
                }
            }
        }
    }
}
