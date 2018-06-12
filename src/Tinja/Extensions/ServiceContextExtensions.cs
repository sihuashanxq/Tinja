using System;
using Tinja.Resolving;
using Tinja.Resolving.Context;

namespace Tinja.Extensions
{
    public static class ServiceContextExtensions
    {
        public static Func<IServiceResolver, object> GetImplementionFactory(this IServiceContext ctx)
        {
            if (ctx is ServiceDelegateContext factoryContext)
            {
                return factoryContext.ImplementionFactory;
            }

            throw new InvalidOperationException($"Type:{ctx.GetType().FullName} can not converted to ServiceFactoryImplTypeContext");
        }

        public static Type GetImplementionType(this IServiceContext ctx)
        {
            if (ctx is ServiceTypeContext implContext)
            {
                return implContext.ImplementionType;
            }

            throw new InvalidOperationException($"Type:{ctx.GetType().FullName} can not converted to ServiceImplTypeContext");
        }

        public static TypeConstructor[] GetConstructors(this IServiceContext ctx)
        {
            if (ctx is ServiceTypeContext implContext)
            {
                return implContext.Constrcutors;
            }

            return new TypeConstructor[0];
        }
    }
}
