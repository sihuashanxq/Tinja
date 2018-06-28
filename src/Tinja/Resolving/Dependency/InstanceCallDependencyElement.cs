namespace Tinja.Resolving
{
    /// AddSingleton(typeof(Serice),new Service());
    public class InstanceCallDependencyElement : CallDepenencyElement
    {
        public object Instance { get; set; }

        protected internal override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitInstance(this);
        }
    }
}
