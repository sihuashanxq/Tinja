namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <inheritdoc />
    /// AddSingleton(typeof(Service),new Service());
    public class InstanceCallDependElement : CallDependElement
    {
        public object Instance { get; set; }

        public  override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitInstance(this);
        }
    }
}
