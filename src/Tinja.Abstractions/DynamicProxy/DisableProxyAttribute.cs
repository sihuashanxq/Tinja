using System;

namespace Tinja.Abstractions.DynamicProxy
{
    /// <summary>
    /// mark a class will not be proxied
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DisableProxyAttribute : Attribute
    {

    }
}
