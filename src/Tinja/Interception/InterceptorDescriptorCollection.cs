using System;
using System.Collections;
using System.Collections.Generic;

namespace Tinja.Interception
{
    public class InterceptorDescriptorCollection : ICollection<InterceptorDescriptor>
    {
        private readonly List<InterceptorDescriptor> _descriptors = new List<InterceptorDescriptor>();

        public int Count => _descriptors.Count;

        public bool IsReadOnly => false;

        public Type ServiceType { get; }

        public Type ImplementionType { get; }

        public InterceptorDescriptorCollection(Type serviceType, Type implementionType)
        {
            if (serviceType == null)
            {
                throw new NullReferenceException(nameof(serviceType));
            }

            if (implementionType == null)
            {
                throw new NullReferenceException(nameof(implementionType));
            }

            ServiceType = serviceType;
            ImplementionType = implementionType;
        }

        public void Add(InterceptorDescriptor item)
        {
            _descriptors.Add(item);
        }

        public void AddRange(IEnumerable<InterceptorDescriptor> descriptors)
        {
            if (descriptors == null)
            {
                return;
            }

            foreach (var item in descriptors)
            {
                Add(item);
            }
        }

        public void Clear()
        {
            _descriptors.Clear();
        }

        public bool Contains(InterceptorDescriptor item)
        {
            return _descriptors.Contains(item);
        }

        public void CopyTo(InterceptorDescriptor[] array, int arrayIndex)
        {
            _descriptors.CopyTo(array, arrayIndex);
        }

        public IEnumerator<InterceptorDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        public bool Remove(InterceptorDescriptor item)
        {
            return _descriptors.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }
    }
}
