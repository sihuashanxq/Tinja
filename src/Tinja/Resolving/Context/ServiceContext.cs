using System;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Context
{
    public class ServiceContext
    {
        public Type ServiceType { get; set; }

        public ServiceLifeStyle LifeStyle { get; set; }

        public static readonly ServiceContext[] Empties = new ServiceContext[0];
    }
}
