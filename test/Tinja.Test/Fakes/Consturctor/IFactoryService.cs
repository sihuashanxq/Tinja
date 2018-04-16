namespace Tinja.Test.Fakes
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
