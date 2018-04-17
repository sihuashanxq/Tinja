namespace Tinja.Test.Fakes
{
    public interface ITransientServiceA
    {
         ITransientServiceC ServiceC { get; }

         ITransientServiceB ServiceB { get; }
    }

    public class TransientServiceA : ITransientServiceA
    {
        public ITransientServiceC ServiceC { get; }

        public ITransientServiceB ServiceB { get; }

        public TransientServiceA(ITransientServiceB serviceB, ITransientServiceC serviceC)
        {
            ServiceC = serviceC;
            ServiceB = serviceB;
        }
    }

    public class TransientServiceA2 : ITransientServiceA
    {
        public ITransientServiceC ServiceC { get; }

        public ITransientServiceB ServiceB { get; }

        public TransientServiceA2(ITransientServiceB serviceB, ITransientServiceC serviceC)
        {
            ServiceC = serviceC;
            ServiceB = serviceB;
        }
    }
}
