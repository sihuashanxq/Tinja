using Tinja.Annotations;

namespace Tinja.Test.Fakes
{
    public interface IPropertyServiceA
    {
        IPropertyCircularInjectionService Service { get; set; }
    }

    public interface IPropertyServiceB
    {
        IPropertyServiceA Service { get; set; }
    }

    public interface IPropertyCircularInjectionService
    {
        IPropertyServiceB Service { get; set; }
    }

    public class PropertyServiceA : IPropertyServiceA
    {
        [Inject]
        public IPropertyCircularInjectionService Service { get; set; }
    }

    public class PropertyServiceB : IPropertyServiceB
    {
        [Inject]
        public IPropertyServiceA Service { get; set; }
    }

    public class PropertyCircularInjectionService : IPropertyCircularInjectionService
    {
        [Inject]
        public IPropertyServiceB Service { get; set; }
    }
}
