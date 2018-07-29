using System;

namespace Tinja.Abstractions.Injection
{
    public class Component
    {
        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public Type ImplementationType { get; set; }

        public object ImplementationInstance { get; set; }

        public Func<IServiceResolver, object> ImplementationFactory { get; set; }

        public override int GetHashCode()
        {
            var hashCode = ServiceType.GetHashCode();

            hashCode += (hashCode * 31) ^ (ImplementationType?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ (ImplementationInstance?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ (ImplementationFactory?.GetHashCode() ?? 0);
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
                    ImplementationType == component.ImplementationType &&
                    ImplementationFactory == component.ImplementationFactory &&
                    ImplementationInstance == component.ImplementationInstance;
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
                ImplementationFactory = ImplementationFactory,
                LifeStyle = LifeStyle,
                ImplementationType = ImplementationType,
                ServiceType = ServiceType,
                ImplementationInstance = ImplementationInstance
            };
        }
    }
}
