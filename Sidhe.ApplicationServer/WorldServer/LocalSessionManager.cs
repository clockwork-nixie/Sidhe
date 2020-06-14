using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.WorldServer
{
    public class LocalSessionManager : ISessionManager
    {
        [NotNull, ItemNotNull] private readonly ConcurrentDictionary<int, Session>[] _partitions;


        public LocalSessionManager([NotNull] IApplicationSettings settings)
        {
            _partitions = Enumerable
                .Range(0, Math.Max(1, (int)settings.LocalSessionPartitionCount))
                .Select(i => new ConcurrentDictionary<int, Session>())
                .ToArray();
        }


        public void Clear()
        {
            foreach (var partition in _partitions)
            {
                foreach (var userId in partition.Keys)
                {
                    DestroySession(userId);
                }
            }
        }


        public Session CreateSession(int userId, bool canReplace = false)
        {
            var sessions = Partition(userId);
            var session = new Session(userId);

            if (!sessions.TryAdd(userId, session))
            {
                if (canReplace)
                {
                    // Destroy the session for the current instance of the user-id
                    // and try to replace it.  If it fails it must mean there are a
                    // lot of valid requests for the same user-id, which is just silly.
                    // We said "can" replace not "will" replace :)
                    DestroySession(userId);

                    if (!sessions.TryAdd(userId, session))
                    {
                        session = null;
                    }
                }
            }

            return session;
        }


        public bool DestroySession(int userId)
        {
            if (Partition(userId).TryRemove(userId, out var session))
            {
                session.Invalidate();
            }

            return session != null;
        }


        public Session FindSession(int userId) => Partition(userId).TryGetValue(userId, out var session)? session: null;


        [NotNull] private ConcurrentDictionary<int, Session> Partition(int userId) => _partitions[userId % _partitions.Length];
    }
}
