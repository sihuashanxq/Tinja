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
}
