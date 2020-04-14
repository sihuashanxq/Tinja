using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy.Registrations;
using Tinja.Abstractions.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    internal static class EventBuilderExtensions
    {
        internal static EventBuilder SetCustomAttributes(this EventBuilder builder, EventInfo eventInfo)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (eventInfo == null)
            {
                throw new ArgumentNullException(nameof(eventInfo));
            }

            foreach (var customAttriute in eventInfo
                .CustomAttributes
                .Where(item => item.AttributeType.IsNotType<InjectAttribute>() && item.AttributeType.IsNotType<InterceptorAttribute>()))
            {
                var attrBuilder = GeneratorUtils.CreateCustomAttribute(customAttriute);
                if (attrBuilder != null)
                {
                    builder.SetCustomAttribute(attrBuilder);
                }
            }

            return builder;
        }
    }
}
