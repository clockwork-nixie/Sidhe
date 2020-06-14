using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.WorldServer
{
    public class World : IWorld
    {
        [NotNull] private readonly ISessionManager _sessions;
        [NotNull] private readonly IZone _zone;
        

        public World([NotNull] IFactory factory)
        {
            _sessions = factory.GetInstance<ISessionManager>();
            _zone =  factory.GetInstance<IZone>();
        }


        public Session BeginSession(int userId) => _sessions.CreateSession(userId);


        public void Dispatch(WorldWorkRequest model) { throw new System.NotImplementedException(); }


        public ClientResponse Execute(ClientRequest request, Session session)
        {
            ClientResponse response;

            if (session == null)
            {
                response = new ClientResponse {ErrorMessage = "Session does not exist."};
            }
            else if (request.Location != null)
            {
                var location = request.Location;

                location.UserId = session.UserId;

                response = new ClientResponse {
                    Changes = _zone.UpdateLocation(location, session)
                };
            }
            else
            {
                response = new ClientResponse {ErrorMessage = "Unknown command."};
            }

            return response;
        }


        public void EndSession(Session session)
        {
            _sessions.DestroySession(session.UserId);
            _zone.Remove(session.SessionId);
        }
    }
}
