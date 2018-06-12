using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Extensions;
using Tinja.Resolving.Context;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Dependency
{
    public class ServiceCallDependency
    {
        public IServiceContext Context { get; set; }

        public TypeConstructor Constructor { get; set; }

        public Dictionary<PropertyInfo, ServiceCallDependency> Properties { get; set; }

        public Dictionary<ParameterInfo, ServiceCallDependency> Parameters { get; set; }

        /// <summary>
        /// Property Circular Dependencies,just Singleton/Scoped
        /// </summary>
        public bool IsPropertyCircularDependencies { get; internal set; }

        public ServiceCallDependency()
        {
            Properties = new Dictionary<PropertyInfo, ServiceCallDependency>();
            Parameters = new Dictionary<ParameterInfo, ServiceCallDependency>();
        }

        public virtual bool ContainsPropertyCircularDependencies()
        {
            if (IsPropertyCircularDependencies)
            {
                return true;
            }

            if (Properties != null)
            {
                foreach (var property in Properties)
                {
                    if (property.Value.ContainsPropertyCircularDependencies())
                    {
                        return true;
                    }
                }
            }

            foreach (var parameter in Parameters)
            {
                if (parameter.Value.ContainsPropertyCircularDependencies())
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool ShouldHoldServiceLife()
        {
            if (Context.LifeStyle != ServiceLifeStyle.Transient)
            {
                return true;
            }

            if (Constructor != null)
            {
                return Constructor.ConstructorInfo.DeclaringType.Is(typeof(IDisposable));
            }

            return true;
        }
    }
}
