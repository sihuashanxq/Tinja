namespace Tinja.Resolving.Dependency
{
    public class ServiceManyCallDependency : ServiceCallDependency
    {
        public ServiceCallDependency[] Elements { get; set; }

        public ServiceManyCallDependency()
            : base()
        {
            Elements = new ServiceCallDependency[0];
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
