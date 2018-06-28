using System;
using Tinja.Resolving.Dependency;

namespace Tinja.Resolving
{
    /// <summary>
    /// a visitor/translator for <see cref="CallDependencyElement"/>
    /// </summary>
    /// <typeparam name="TVisitResult">the type for transalted result</typeparam>
    public abstract class CallDependencyElementVisitor<TVisitResult>
    {
        public virtual TVisitResult Visit(CallDepenencyElement element)
        {
            if (element == null)
            {
                return default(TVisitResult);
            }

            return element.Accept(this);
        }

        protected internal abstract TVisitResult VisitMany(ManyCallDepenencyElement element);

        protected internal abstract TVisitResult VisitInstance(InstanceCallDependencyElement element);

        protected internal abstract TVisitResult VisitDelegate(DelegateCallDepenencyElement element);

        protected internal abstract TVisitResult VisitConstrcutor(ConstructorCallDependencyElement element);
    }
}
