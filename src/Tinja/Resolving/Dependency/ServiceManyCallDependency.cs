namespace Tinja.Resolving.Dependency
{
    public class ServiceManyCallDependency : ServiceCallDependency
    {
        public ServiceCallDependency[] Elements { get; set; }

        public ServiceManyCallDependency()
        {
            Elements = new ServiceCallDependency[0];
        }

        public override bool ContainsPropertyCircular()
        {
            foreach (var item in Elements)
            {
                if (item.ContainsPropertyCircular())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
