using System;
using Tinja.Core.Injection.Dependencies;

namespace Tinja.Core.Injection
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        ///if true, throw <see cref="ResolveRequiredPropertyFailedException"/> when resolve property depenency failed!
        /// </summary>
        public bool Requrired { get; set; }
    }
}
