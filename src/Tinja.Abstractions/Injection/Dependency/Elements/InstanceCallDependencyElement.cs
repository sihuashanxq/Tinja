namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// AddSingleton(typeof(Service),new Service());
    public class InstanceCallDependencyElement : CallDepenencyElement
    {
        public object Instance { get; set; }

        public  override TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitInstance(this);
        }
    }
}
