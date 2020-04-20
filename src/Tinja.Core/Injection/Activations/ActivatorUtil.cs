using System;
using System.Reflection;
using Tinja.Abstractions.Injection;

namespace Tinja.Core.Injection.Activations
{
    internal static class ActivatorUtil
    {
        internal static MethodInfo GetLazyValueFactoryMethod { get; }

        internal static ActivatorDelegate DefaultActivator = (r, s) => null;

        static ActivatorUtil()
        {
            GetLazyValueFactoryMethod = typeof(ActivatorUtil).GetMethod(nameof(GetLazyValueFactory), BindingFlags.Static | BindingFlags.Public);
        }

        public static Func<TValue> GetLazyValueFactory<TValue>(ServiceLifeScope scope, string tag, bool tagOptional)
        {
            return () => (TValue)scope.InternalServiceResolver.ResolveService(typeof(TValue), tag, tagOptional);
        }
    }
}
