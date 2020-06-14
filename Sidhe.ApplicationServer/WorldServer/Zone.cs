using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.WorldServer
{
    public class Zone : IZone
    {
        [NotNull] private readonly object _lock = new object();
        [NotNull] private readonly IDictionary<int, ClientLocation> _states = new Dictionary<int, ClientLocation>();
        private uint _locationId = 0;


        public void Remove(int sessionId) { lock (_lock) { _states.Remove(sessionId); } }


        public ClientLocation[] UpdateLocation(ClientLocation location, Session session)
        {
            lock (_lock)
            {
                var previousId = _states
                    .TryGetValue(session.SessionId, out var previous)?
                    previous?.LocationId: 0;

                var changes = _states.Values
                    .Where(s => s.UserId != session.UserId && s.LocationId > previousId)
                    .ToArray();

                if (previous == null || previous.X != location.X || previous.Y != location.Y)
                {
                    ++_locationId;
                }

                location.LocationId = _locationId;
                _states[session.SessionId] = location;

                session.X = location.X;
                session.Y = location.Y;

                return changes;
            }
        }
    }
}
