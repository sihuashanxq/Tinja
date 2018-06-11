namespace Tinja.Test.Fakes.Consturctor
{
    public interface ISingletonServiceA
    {
        ISingletonServiceB ServiceB { get; }

        ISingletonServiceC ServiceC { get; }
    }

    public class SingletonServiceA : ISingletonServiceA
    {
        public ISingletonServiceB ServiceB { get; }

        public ISingletonServiceC ServiceC { get; }

        public SingletonServiceA(ISingletonServiceB serviceB, ISingletonServiceC serviceC)
        {
            ServiceB = serviceB;
            ServiceC = serviceC;
        }
    }
}
