using System.Threading;

namespace Tinja.Core.Injection.Descriptors.Internals
{
    internal class SeviceIdFactory
    {
        private long _seed;

        internal long CreateSeviceId()
        {
            return Interlocked.Increment(ref _seed);
        }
    }
}
