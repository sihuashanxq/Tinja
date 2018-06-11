namespace Tinja.Test.Fakes.Consturctor
{
    public interface IFactoryService
    {
        ITransientServiceA Service { get; }
    }

    public class FactoryService : IFactoryService
    {
        public FactoryService(ITransientServiceA serviceA)
        {
            Service = serviceA;
        }

        public ITransientServiceA Service { get; }
    }
}
