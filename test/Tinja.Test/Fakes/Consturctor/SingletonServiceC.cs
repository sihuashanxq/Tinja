namespace Tinja.Test.Fakes.Consturctor
{
    public interface ISingletonServiceC
    {
        ITransientServiceC TransientServiceC { get; }
    }

    public class SingletonServiceC : ISingletonServiceC
    {
        public ITransientServiceC TransientServiceC { get; }

        public SingletonServiceC(ITransientServiceC transientServiceC)
        {
            TransientServiceC = transientServiceC;
        }
    }
}
