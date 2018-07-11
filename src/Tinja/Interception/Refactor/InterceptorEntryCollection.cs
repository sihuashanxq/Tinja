using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tinja.Interception.Refactor
{
    public class InterceptorEntryCollection : ICollection<InterceptorEntry>
    {
        private readonly List<InterceptorEntry> _entries;

        public InterceptorEntryCollection()
        {
            _entries = new List<InterceptorEntry>();
        }

        public int Count => _entries.Count;

        public bool IsReadOnly => false;

        public void Add(InterceptorEntry item)
        {

        }

        public void Clear()
        {

        }

        public bool Contains(InterceptorEntry item)
        {
            return _entries.Contains(item);
        }

        public void CopyTo(InterceptorEntry[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<InterceptorEntry> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(InterceptorEntry item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
