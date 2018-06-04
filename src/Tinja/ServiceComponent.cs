using System;
using Tinja.Resolving;
using Tinja.ServiceLife;

namespace Tinja
{
    public class ServiceComponent
    {
        public Type ProxyType { get; set; }

        public Type ServiceType { get; set; }

        public Type ImplementionType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public Func<IServiceResolver, object> ImplementionFactory { get; set; }

        public override int GetHashCode()
        {
            var hashCode = ServiceType.GetHashCode();

            hashCode += (hashCode * 31) ^ (ImplementionType?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ (ImplementionFactory?.GetHashCode() ?? 0);
            hashCode += (hashCode * 31) ^ LifeStyle.GetHashCode();
            hashCode += (hashCode * 31) ^ (ProxyType?.GetHashCode() ?? 0);

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is ServiceComponent component)
            {
                return
                    LifeStyle == component.LifeStyle &&
                    ServiceType == component.ServiceType &&
                    ImplementionType == component.ImplementionType &&
                    ImplementionFactory == component.ImplementionFactory &&
                    ProxyType == component.ProxyType;
            }

            return false;
        }

        public static bool operator ==(ServiceComponent left, ServiceComponent right)
        {
            if (!(left is null))
            {
                return left.Equals(right);
            }

            if (!(right is null))
            {
                return right.Equals(left);
            }

            return true;
        }

        public static bool operator !=(ServiceComponent left, ServiceComponent right)
        {
            return !(left == right);
        }

        public ServiceComponent Clone()
        {
            return new ServiceComponent()
            {
                ProxyType = ProxyType,
                ImplementionFactory = ImplementionFactory,
                LifeStyle = LifeStyle,
                ImplementionType = ImplementionType,
                ServiceType = ServiceType
            };
        }
    }
}
