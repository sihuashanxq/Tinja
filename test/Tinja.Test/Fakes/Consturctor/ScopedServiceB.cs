namespace Tinja.Test.Fakes.Consturctor
{
    public interface IScopedServiceB
    {
        ITransientServiceC TransientServiceC { get; }
    }

    public class ScopedServiceB : IScopedServiceB
    {
        public ITransientServiceC TransientServiceC { get; }

        public ScopedServiceB(ITransientServiceC transientServiceC)
        {
            TransientServiceC = transientServiceC;
        }
    }
}
