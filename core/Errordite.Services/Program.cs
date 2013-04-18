using System;
using System.Diagnostics;
using CodeTrip.Core.Extensions;
using CodeTrip.Core.IoC;
using Magnum.CommandLineParser;
using Topshelf;

namespace Errordite.Services
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var instance = ParseInstanceName(Environment.CommandLine);

                Trace.Write("Attempting to start Asos.StyleManagement.Integration${0}".FormatWith(instance));

                if (instance == null)
                {
                    Trace.Write("Failed to load instance configuration from container");
                    return;
                }

                var configuration = ObjectFactory.GetObject<ServiceConfiguration>(instance);

                Trace.Write("Loaded configuration, ServiceName:={0}, QueuePath:={1}, MachineName:={2}".FormatWith(
                    configuration.ServiceName,
                    configuration.QueueAddress,
                    configuration.MachineName));

                HostFactory.Run(c =>
                {
                    c.StartAutomatically();
                    c.SetServiceName(configuration.ServiceName);
                    c.SetDisplayName(configuration.ServiceDisplayName);
                    c.SetDescription(configuration.ServiceDiscription);

                    c.DependsOnMsmq();
                    c.DependsOnEventLog();

                    c.UseLog4Net(@"config\log4net.config");

                    if (configuration.Username.IsNullOrEmpty())
                        c.RunAsPrompt();
                    else
                        c.RunAs(configuration.Username, configuration.Password);

                    c.Service<ErrorditeService>(s =>
                    {
                        s.ConstructUsing(builder => new ErrorditeService(configuration, instance));
                        s.WhenStarted(svc => svc.Start());
                        s.WhenStopped(svc =>
                        {
                            svc.Stop(instance);
                            ObjectFactory.Container.Dispose();
                        });
                    });
                });
            }
            catch (Exception e)
            {
                Trace.Write(e.ToString());
            }
        }

        /// <summary>
        ///   Parses the command line
        /// </summary>
        /// <param name="commandLine"> The command line to parse </param>
        /// <returns> The command line elements that were found </returns>
        private static string ParseInstanceName(string commandLine)
        {
            var parser = new StringCommandLineParser();
            var result = parser.All(commandLine);

            while (result != null)
            {
                var element = result.Value as DefinitionElement;

                if (element != null && element.Key.ToLowerInvariant() == "instance")
                {
                    return element.Value;
                }

                result = parser.All(result.Rest);
            }

            return null;
        }
    }
}
