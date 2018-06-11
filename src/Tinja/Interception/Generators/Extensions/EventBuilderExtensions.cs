using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Tinja.Interception.Generators.Extensions
{
    public static class EventBuilderExtensions
    {
        public static EventBuilder SetCustomAttributes(this EventBuilder builder, EventInfo eventInfo)
        {
            if (builder == null || eventInfo == null)
            {
                return builder;
            }

            foreach (var customAttriute in eventInfo
                .CustomAttributes
                .Where(item => item.AttributeType != typeof(InjectAttribute) && item.AttributeType != typeof(InterceptorAttribute)))
            {
                var attributeBuilder = GeneratorUtility.CreateCustomAttribute(customAttriute);
                if (attributeBuilder != null)
                {
                    builder.SetCustomAttribute(attributeBuilder);
                }
            }

            return builder;
        }
    }
}
