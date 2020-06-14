using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading.Tasks;
using DependencyInjection;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Sidhe.ApplicationServer;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.ApplicationServer.Network;
using Sidhe.ApplicationServer.Terminal;
using Sidhe.ApplicationServer.WorldServer;
using Sidhe.LoginServer;
using Sidhe.LoginServer.Interfaces;
using Sidhe.Utilities;
using Sidhe.Utilities.Interfaces;
using Sidhe.Webservice.Model;

namespace Sidhe.Webservice
{
    public class Startup
    {
        [UsedImplicitly] public Startup(IConfiguration configuration) => Configuration = configuration;

        
        [UsedImplicitly] public IConfiguration Configuration { get; }


        [UsedImplicitly]
        public void ConfigureServices(IServiceCollection services)
        {
            IApplicationSettings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
            var loginCache = new LoginCache();

            services?.AddControllers();
            services?.AddSingleton(settings);
            services?.AddSingleton(CreateApplicationServer(settings, loginCache));
            services?.AddSingleton(CreateLoginServer(settings, loginCache));
        }
        

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints?.MapControllers());
            app.UseStaticFiles();
            app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120),  ReceiveBufferSize = 4 * 1024 });
            app.Use(HandleSocketSession);
        }


        [NotNull]
        private static IApplicationService CreateApplicationServer([NotNull] IApplicationSettings settings, [NotNull] ILoginCache loginCache)
        {
            IFactoryBuilder builder = new FactoryBuilder();

            builder.RegisterInstance(settings);
            builder.RegisterInstance(loginCache);
            builder.RegisterSingleton<IApplicationService, ApplicationService>();
            builder.RegisterSingleton<IClientCache, ClientCache>();

            builder
                .Register<IApplicationManager, ApplicationManager>()
                .Register<IConsole, ConsoleWrapper>()
                .Register<IConsoleInterpreter, ConsoleInterpreter>()
                .Register<IConsoleWorker, ConsoleWorker>()
                .Register<ISessionManager, LocalSessionManager>()
                .Register<IWorld, World>()
                .Register<IWorldWorker, IWorld, IWorldWorkQueue>((world, queue) => new WorldWorker(builder.Factory, world, queue))
                .Register<IWorldWorkQueue, WorldWorkQueue>()
                .Register<IQueueWorkerSet<WorldWorkRequest, IWorldWorker, IWorldWorkQueue>, QueueWorkerSet<WorldWorkRequest, IWorldWorker, IWorldWorkQueue>>()
                .Register<IZone, Zone>();

            return builder.Factory.GetInstance<IApplicationService>();
        }


        [NotNull]
        private static ILoginService CreateLoginServer([NotNull] IApplicationSettings settings, [NotNull] ILoginRegistry loginCache)
        {
            IFactoryBuilder builder = new FactoryBuilder();

            builder.RegisterInstance(settings);
            builder.RegisterInstance(loginCache);
            builder.RegisterSingleton<ILoginService, LoginService>();
            builder.RegisterSingleton<ILoginAuthenticator, LoginAuthenticator>();
            
            return builder.Factory.GetInstance<ILoginService>();
        }


        private static async Task HandleSocketSession(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path == "/client" && context.WebSockets.IsWebSocketRequest)
            {
                var socket = await context.WebSockets.AcceptWebSocketAsync();

                if (socket?.State == WebSocketState.Open)
                {
                    var application = (IApplicationService) context.RequestServices.GetService(typeof(IApplicationService));
                    var client = new WebSocketClient(new WebSocketWrapper(socket), application.Settings);

                    await application.RunClient(client);
                }
            }
            else
            {
                await next();
            }
        }
    }
}
