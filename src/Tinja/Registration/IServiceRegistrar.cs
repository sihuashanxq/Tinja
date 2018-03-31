using System;

namespace Tinja.Registration
{
    public interface IServiceRegistrar
    {
        void Register(Type serviceType, Type implType, LifeStyle lifeStyle);

        void Register(Type serviceType, Func<IContainer, object> implFacotry, LifeStyle lifeStyle);
    }
}
