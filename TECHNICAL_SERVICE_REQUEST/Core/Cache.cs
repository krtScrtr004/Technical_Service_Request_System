using System;
using System.Linq;
using System.Runtime.Caching;

namespace TROUBLESHOOTING_REPAIR_SERVICE_REQUEST_SYSTEM.Core
{
    public class Cache
    {
        private static readonly ObjectCache _Cache = MemoryCache.Default;

        public static T GetOrAdd<T>(string key, Func<T> factory, int minutes = 10)
        {
            if (_Cache.Contains(key))
            {
                return (T)_Cache[key];
            }

            var value = factory();
            _Cache.Set(key, value, DateTimeOffset.UtcNow.AddMinutes(minutes));
            return value;
        }

        public static void Remove(string key)
        {
            _Cache.Remove(key);
        }
    }
}