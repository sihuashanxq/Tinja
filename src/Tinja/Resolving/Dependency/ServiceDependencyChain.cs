using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Dependency
{
    public class ServiceDependencyChain
    {
        public IServiceResolvingContext Context { get; set; }

        public TypeConstructorMetadata Constructor { get; set; }

        public Dictionary<PropertyInfo, ServiceDependencyChain> Properties { get; set; }

        public Dictionary<ParameterInfo, ServiceDependencyChain> Parameters { get; set; }

        /// <summary>
        /// Property Circular Dependencies,just Singleton/Scoped
        /// </summary>
        public bool IsPropertyCircularDependencies { get; internal set; }

        public ServiceDependencyChain()
        {
            Properties = new Dictionary<PropertyInfo, ServiceDependencyChain>();
            Parameters = new Dictionary<ParameterInfo, ServiceDependencyChain>();
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
            if (Context.Component.LifeStyle != ServiceLifeStyle.Transient)
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
