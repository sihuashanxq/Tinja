namespace Tinja.Abstractions.Configurations
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// an asbtraction of Configuration
    /// </summary>
    public abstract class Configuration : IConfiguration
    {
        protected ConcurrentDictionary<string, object> Values { get; }

        public Configuration()
        {
            Values = new ConcurrentDictionary<string, object>();
        }

        public virtual void Clear()
        {
            Values.Clear();
        }

        public virtual bool Remove(string key)
        {
            if (key == null)
            {
                throw new NullReferenceException(nameof(key));
            }

            return Values.TryRemove(key, out var _);
        }

        public virtual IEnumerable<object> Get()
        {
            return Values.Values;
        }

        public virtual TValue Get<TValue>(string key)
        {
            if (Values.TryGetValue(key, out var value))
            {
                if (value is TValue)
                {
                    return (TValue)value;
                }

                if (value is null)
                {
                    return default(TValue);
                }

                throw new InvalidCastException($"Value of type:{value.GetType().FullName} can not cast to:{typeof(TValue).FullName}");
            }

            return default(TValue);
        }

        public virtual bool Set(string key, object value)
        {
            if (key == null)
            {
                throw new NullReferenceException(nameof(key));
            }

            return Values.TryAdd(key, value);
        }
    }
}
