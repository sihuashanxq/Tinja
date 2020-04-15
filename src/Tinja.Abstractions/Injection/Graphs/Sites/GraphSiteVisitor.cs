namespace Tinja.Abstractions.Injection.Graphs.Sites
{
    /// <summary>
    /// a visitor/translator for <see cref="GraphSite"/>
    /// </summary>
    /// <typeparam name="TVisitResult">the type for translated result</typeparam>
    public abstract class GraphSiteVisitor<TVisitResult>
    {
        public virtual TVisitResult Visit(GraphSite site)
        {
            if (site == null)
            {
                return default(TVisitResult);
            }

            return site.Accept(this);
        }


        protected internal abstract TVisitResult VisitType(GraphTypeSite site);

        protected internal abstract TVisitResult VisitLazy(GraphLazySite site);

        protected internal abstract TVisitResult VisitConstant(GraphConstantSite site);

        protected internal abstract TVisitResult VisitInstance(GraphInstanceSite site);

        protected internal abstract TVisitResult VisitDelegate(GraphDelegateSite site);

        protected internal abstract TVisitResult VisitEnumerable(GraphEnumerableSite site);

        protected internal abstract TVisitResult VisitValueProvider(GraphValueProviderSite site);
    }
}
