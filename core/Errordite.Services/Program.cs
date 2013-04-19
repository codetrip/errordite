using System;
using System.Diagnostics;
using Castle.MicroKernel.Registration;
using Errordite.Core.Extensions;
using Errordite.Core.IoC;
using Errordite.Services.Configuration;
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

                Trace.Write("Attempting to start Errordite.Services${0}".FormatWith(instance));

                if (instance == null)
                {
                    Trace.Write("Failed to load instance configuration from container");
                    return;
                }

                var configuration = ObjectFactory.GetObject<ServiceConfiguration>(instance.ToString());

                Trace.Write("Loaded configuration, Service:={0}, Queue:={1}".FormatWith(
                    configuration.ServiceName,
                    configuration.QueueAddress));

                //register our configuration for this service
                ObjectFactory.Container.Register(Component
                    .For<ServiceConfigurationContainer>()
                    .ImplementedBy(typeof (ServiceConfigurationContainer))
                    .LifestyleSingleton());

                HostFactory.Run(c =>
                {
                    c.StartAutomatically();
                    c.SetServiceName(configuration.ServiceName);
                    c.SetDisplayName(configuration.ServiceDisplayName);
                    c.SetDescription(configuration.ServiceDiscription);
                    c.DependsOnEventLog();

                    c.UseLog4Net(@"config\log4net.config");

                    if (configuration.Username.IsNullOrEmpty())
                        c.RunAsPrompt();
                    else
                        c.RunAs(configuration.Username, configuration.Password);

                    c.Service<IErrorditeService>(s =>
                    {
                        s.ConstructUsing(builder => ObjectFactory.GetObject<IErrorditeService>());
                        s.WhenStarted(svc => svc.Start());
                        s.WhenStopped(svc =>
                        {
                            svc.Stop();
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
        private static ServiceInstance? ParseInstanceName(string commandLine)
        {
            var parser = new StringCommandLineParser();
            var result = parser.All(commandLine);

            while (result != null)
            {
                var element = result.Value as DefinitionElement;

                if (element != null && element.Key.ToLowerInvariant() == "instance")
                {
                    return (ServiceInstance)Enum.Parse(typeof(ServiceInstance), element.Value, true);
                }

                result = parser.All(result.Rest);
            }

            return null;
        }
    }
}
