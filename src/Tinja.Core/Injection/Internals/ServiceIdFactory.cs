using System;
using System.Threading;

namespace Tinja.Core.Injection.Internals
{
    internal class ServiceIdFactory
    {
        private int _seed;

        internal int CreateSeviceId()
        {
            return Interlocked.Increment(ref _seed);
        }
    }
}
