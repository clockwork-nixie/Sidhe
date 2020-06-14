using System.Collections.Concurrent;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;

namespace Sidhe.ApplicationServer
{
    public class ClientCache : IClientCache
    {
        [NotNull] private readonly ConcurrentDictionary<int, IApplicationClient> _cache = new ConcurrentDictionary<int, IApplicationClient>();
        [NotNull] private readonly object _lock = new object();


        public IApplicationClient AddOrReplace(IApplicationClient client)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_cache.TryGetValue(client.UserId, out var existing) && existing == client)
            {
                return null;
            }

            lock (_lock)
            {
                if (_cache.TryGetValue(client.UserId, out existing) && existing == client)
                {
                    return null;
                }

                _cache[client.UserId] = client;

                return existing;
            }
        }


        public IApplicationClient Find(int userId) => _cache.TryGetValue(userId, out var found) ? found : null;
    

        public bool Remove(IApplicationClient client)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_cache.TryGetValue(client.UserId, out var existing) && existing == client)
            {
                return false;
            }

            lock (_lock)
            {
                if (_cache.TryGetValue(client.UserId, out existing) && existing == client)
                {
                    return _cache.TryRemove(client.UserId, out _);
                }

                return false;
            }
        }
    }
}
