namespace Tinja.Resolving.Dependency
{
    public class ServiceEnumerableDependChain : ServiceDependChain
    {
        public ServiceDependChain[] Elements { get; set; }

        public ServiceEnumerableDependChain() 
            : base()
        {
            Elements = new ServiceDependChain[0];
        }
    }
}
