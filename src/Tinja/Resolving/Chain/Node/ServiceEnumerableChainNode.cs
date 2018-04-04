namespace Tinja.Resolving.Chain.Node
{
    public class ServiceEnumerableChainNode : ServiceConstrutorChainNode
    {
        public IServiceChainNode[] Elements { get; set; }
    }
}
