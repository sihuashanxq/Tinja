namespace Tinja.Resolving.Dependency
{
    public class ServiceDependencyEnumerableChain : ServiceDependencyChain
    {
        public ServiceDependencyChain[] Elements { get; set; }

        public ServiceDependencyEnumerableChain()
            : base()
        {
            Elements = new ServiceDependencyChain[0];
        }

        public override bool ContainsPropertyCircularDependencies()
        {
            foreach (var item in Elements)
            {
                if (item.ContainsPropertyCircularDependencies())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
