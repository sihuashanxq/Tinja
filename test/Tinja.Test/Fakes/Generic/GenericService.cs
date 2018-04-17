using System;
using System.Collections.Generic;
using System.Text;

namespace Tinja.Test.Fakes
{
    public interface IGenericService<TService>
    {
        TService Service { get; }
    }

    public interface IGenericService2<TService>
    {

    }

    public class GenericService<TService> : IGenericService<TService>
    {
        public TService Service { get; }

        public GenericService(TService service)
        {
            Service = service;
        }
    }

    public class GenericService2<TSerivce> : IGenericService2<TSerivce>
    {

    }
}
