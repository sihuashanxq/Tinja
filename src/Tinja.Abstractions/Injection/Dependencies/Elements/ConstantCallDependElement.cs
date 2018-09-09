namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <inheritdoc />
    /// <summary>
    /// public MyClass(string name="")
    /// </summary>
    public class ConstantCallDependElement : CallDependElement
    {
        public object Constant { get; set; }

        public override TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            return visitor.VisitConstant(this);
        }
    }
}
