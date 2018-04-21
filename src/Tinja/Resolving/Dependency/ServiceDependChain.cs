using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.LifeStyle;
using Tinja.Resolving.Context;

namespace Tinja.Resolving.Dependency
{
    public class ServiceDependChain
    {
        public IResolvingContext Context { get; set; }

        public ServiceConstructorInfo Constructor { get; set; }

        public Dictionary<PropertyInfo, ServiceDependChain> Properties { get; set; }

        public Dictionary<ParameterInfo, ServiceDependChain> Parameters { get; set; }

        /// <summary>
        /// Property Circular Dependencies,just Singleton/Scoped
        /// </summary>
        public bool IsPropertyCircularDependencies { get; internal set; }

        public ServiceDependChain()
        {
            Properties = new Dictionary<PropertyInfo, ServiceDependChain>();
            Parameters = new Dictionary<ParameterInfo, ServiceDependChain>();
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

        public virtual bool IsNeedWrappedLifeStyle()
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
