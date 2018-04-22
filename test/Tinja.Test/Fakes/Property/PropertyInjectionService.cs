namespace Tinja.Test.Fakes
{
    public interface IPropertyInjectionService
    {
        ITransientServiceB ServiceB { get; set; }
    }

    public class PropertyInjectionService : IPropertyInjectionService
    {
        [Inject]
        public ITransientServiceB ServiceB { get; set; }
    }
}
