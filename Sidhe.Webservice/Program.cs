using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.LoginServer.Interfaces;

namespace Sidhe.Webservice
{
    public class Program
    {
        [NotNull] private static readonly NLog.Logger Logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();


        public static void Main(string[] args)
        {
            try
            {
                Logger.Debug("*** APPLICATION STARTING ***");
                var host = CreateHostBuilder(args).Build();

                Boot(host.Services);
                host.Run();
            }
            catch (Exception exception)
            {
                Logger.Fatal(exception, "*** APPLICATION TERMINATION DUE TO UNHANDLED EXCEPTION ***");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }


        public static void Boot([NotNull] IServiceProvider services)
        {
            var settings = (IApplicationSettings) services.GetService(typeof(IApplicationSettings));
            var applicationServer = settings.IsHostApplication ? (IApplicationService) services.GetService(typeof(IApplicationService)) : null;
            var loginServer = settings.IsHostLogin ? (ILoginService) services.GetService(typeof(ILoginService)) : null;
            
            applicationServer?.Startup();
            loginServer?.Startup();
        }


        [NotNull]
        private static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseStartup<Startup>()
                    .UseWebRoot(@".\Public")
                    .UseKestrel(options => options.ListenAnyIP(5001, o => o.UseHttps())))
            .ConfigureLogging(logging => {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .UseNLog();
    }
}
