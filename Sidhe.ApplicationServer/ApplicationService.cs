using System;
using System.Threading;
using System.Threading.Tasks;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer
{
    public class ApplicationService : Dependency, IApplicationService
    {
        [NotNull] private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [NotNull] private readonly IClientCache _clients;
        [NotNull] private readonly ILoginCache _logins;
        [NotNull] private readonly object _lock = new object();
        private volatile IApplicationManager _manager;
        

        public ApplicationService([NotNull] IFactory factory,
            [NotNull] IApplicationSettings settings, [NotNull] ILoginCache logins, [NotNull] IClientCache clients) 
            : base(factory)
        {
            _clients = clients;
            _logins = logins;
            Settings = settings;

            if (settings.IsConsoleEnabled)
            {
                Logger.Info("Creating console for application.");
                
                var console = GetInstance<IConsoleWorker>();
                
                console.OnSessionRequest += ConsoleDispatch;
                console.Start(CancellationToken.None);
            }
        }


        private ApplicationResponse ConsoleDispatch([NotNull] ApplicationRequest request)
        {
            try
            {
                return new ApplicationResponse {ErrorMessage = "No longer implemented."};
            }
            catch (Exception exception)
            {
                return new ApplicationResponse {ErrorMessage = exception.Message};
            }
        }


        private ClientResponse Execute([NotNull] ClientRequest request, [NotNull] Session session)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var manager = _manager;

            return manager?.Execute(request, session) ?? new ClientResponse {
                ErrorMessage = "The world does not yet exist."
            };
        }


        private Session OnAddClient([NotNull] IApplicationClient client)
        {
            var removed = _clients.AddOrReplace(client);

            removed?.Terminate();

            var manager = _manager;

            return manager?.BeginSession(client.UserId);
        }


        private void OnRemoveClient([NotNull] IApplicationClient client)
        {
            _clients.Remove(client);
        }

        
        public void Recycle()
        {
            Shutdown();
            Startup();
        }


        /// <summary>
        /// Must only be called on a web-socket thread.
        /// </summary>
        public async Task RunClient(IApplicationClient client)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var manager = _manager;
            Session session = null;

            if (manager != null && await client.Initialise(OnRemoveClient) &&
                _logins.TryGetValue(client.UserId, out var loginId) &&
                string.Equals(loginId, client.LoginId))
            {
                lock (_lock)
                {
                    if (manager == _manager)
                    {
                        session = OnAddClient(client);
                    }
                }

                if (session != null)
                {
                    await client.Run(Execute, session);
                }
            }
            client.Terminate();
        }


        public IApplicationSettings Settings { get; }


        public void Shutdown()
        {
            IApplicationManager manager;

            lock (_lock)
            {
                manager = _manager ?? throw new InvalidOperationException("Not yet booted.");
                _manager = null;
            }
                            
            manager?.Stop();
            manager?.FinishedHandle.WaitOne();
            manager?.Dispose();
        }


        public void Startup()
        {
            lock (_lock)
            {
                if (_manager != null)
                {
                    throw new InvalidOperationException("Already booted.");
                }

                _manager = GetInstance<IApplicationManager>();
                Task.Run(() => _manager.Run());
            }
        }
    }
}
