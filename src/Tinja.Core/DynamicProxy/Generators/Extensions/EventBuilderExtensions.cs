using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Abstractions.DynamicProxy;
using Tinja.Abstractions.Injection.Extensions;
using Tinja.Core.Injection;

namespace Tinja.Core.DynamicProxy.Generators.Extensions
{
    public static class EventBuilderExtensions
    {
        public static EventBuilder SetCustomAttributes(this EventBuilder builder, EventInfo eventInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (eventInfo == null)
            {
                throw new NullReferenceException(nameof(eventInfo));
            }

            foreach (var customAttriute in eventInfo
                .CustomAttributes
                .Where(item => !item.AttributeType.Is(typeof(InjectAttribute)) &&
                               !item.AttributeType.Is(typeof(InterceptorAttribute))))
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
