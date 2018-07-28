namespace Tinja.Abstractions.Configurations
{
    using System.Collections.Generic;

    public interface IConfiguration
    {
        void Clear();

        bool Remove(string key);

        IEnumerable<object> Get();

        TValue Get<TValue>(string key);

        bool Set(string key, object value);
    }
}
