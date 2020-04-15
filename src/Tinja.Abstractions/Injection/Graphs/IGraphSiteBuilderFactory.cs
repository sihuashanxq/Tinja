namespace Tinja.Abstractions.Injection.Graphs
{
    /// <summary>
    /// an interface for create <see cref="IGraphSiteBuilder"/>
    /// </summary>
    public interface IGraphSiteBuilderFactory
    {
        IGraphSiteBuilder Create();
    }
}
