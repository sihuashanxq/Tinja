namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <inheritdoc />
    /// AddSingleton(typeof(Service),new Service());
    public class GraphInstanceSite : GraphSite
    {
        public object Instance { get; set; }

        public  override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitInstance(this);
        }
    }
}
