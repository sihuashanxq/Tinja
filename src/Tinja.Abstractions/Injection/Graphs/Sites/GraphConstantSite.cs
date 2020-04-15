namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <inheritdoc />
    /// <summary>
    /// public MyClass(string name="")
    /// </summary>
    public class GraphConstantSite : GraphSite
    {
        public object Constant { get; set; }

        public override TVisitResult Accept<TVisitResult>(GraphSiteVisitor<TVisitResult> visitor)
        {
            return visitor.VisitConstant(this);
        }
    }
}
