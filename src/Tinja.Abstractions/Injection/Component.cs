using System;

namespace Tinja.Abstractions.Injection
{
    public class Component
    {
        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public Type ImplementionType { get; set; }

        public object ImplementionInstance { get; set; }

        public Func<IServiceResolver, object> ImplementionFactory { get; set; }

        public override int GetHashCode()
        {
            var hashCode = ServiceType.GetHashCode();

            hashCode += (hashCode * 31) ^ (ImplementionType?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ (ImplementionInstance?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ (ImplementionFactory?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ LifeStyle.GetHashCode();

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is Component component)
            {
                return
                    LifeStyle == component.LifeStyle &&
                    ServiceType == component.ServiceType &&
                    ImplementionType == component.ImplementionType &&
                    ImplementionFactory == component.ImplementionFactory &&
                    ImplementionInstance == component.ImplementionInstance;
            }

            return false;
        }

        public static bool operator ==(Component left, Component right)
        {
            if (!ReferenceEquals(left, null))
            {
                return left.Equals(right);
            }

            return ReferenceEquals(right, null);
        }

        public static bool operator !=(Component left, Component right)
        {
            return !(left == right);
        }

        public Component Clone()
        {
            return new Component()
            {
                ImplementionFactory = ImplementionFactory,
                LifeStyle = LifeStyle,
                ImplementionType = ImplementionType,
                ServiceType = ServiceType,
                ImplementionInstance = ImplementionInstance
            };
        }
    }
}
