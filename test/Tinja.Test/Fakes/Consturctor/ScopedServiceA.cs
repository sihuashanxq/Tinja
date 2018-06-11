namespace Tinja.Test.Fakes.Consturctor
{
    public interface IScopeServiceA
    {
        IScopedServiceB ServiceB { get; }

        IScopedServiceC ServiceC { get; }
    }

    public class ScopedServiceA : IScopeServiceA
    {
        public IScopedServiceB ServiceB { get; }

        public IScopedServiceC ServiceC { get; }

        public ScopedServiceA(IScopedServiceB serviceB, IScopedServiceC serviceC)
        {
            ServiceB = serviceB;
            ServiceC = serviceC;
        }
    }
}
