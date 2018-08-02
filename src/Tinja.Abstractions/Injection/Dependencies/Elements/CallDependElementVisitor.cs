namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <summary>
    /// a visitor/translator for <see cref="CallDependElement"/>
    /// </summary>
    /// <typeparam name="TVisitResult">the type for translated result</typeparam>
    public abstract class CallDependElementVisitor<TVisitResult>
    {
        public virtual TVisitResult Visit(CallDependElement element)
        {
            if (element == null)
            {
                return default(TVisitResult);
            }

            return element.Accept(this);
        }

        protected internal abstract TVisitResult VisitEnumerable(EnumerableCallDependElement element);

        protected internal abstract TVisitResult VisitInstance(InstanceCallDependElement element);

        protected internal abstract TVisitResult VisitDelegate(DelegateCallDependElement element);

        protected internal abstract TVisitResult VisitType(TypeCallDependElement element);
    }
}
