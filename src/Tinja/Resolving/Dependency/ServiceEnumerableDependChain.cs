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

        public override bool ContainsPropertyInjection()
        {
            foreach (var item in Elements)
            {
                if (item.ContainsPropertyInjection())
                {
                    return true;
                }
            }

            return false;
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
