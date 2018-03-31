using System;
using System.Collections.Generic;

namespace Tinja
{
    public class Container : IContainer
    {
        public object Resolve(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Type implType, LifeStyle lifeStyle)
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, Func<IContainer, object> implFacotry, LifeStyle lifeStyle)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
