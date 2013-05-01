using System;
using System.Diagnostics;
using Errordite.Core.Extensions;
using Errordite.Core.IoC;
using Errordite.Tasks.IoC;
using Errordite.Tasks.Tasks;
using Magnum.CommandLineParser;

namespace Errordite.Tasks
{
    public class Program
    {
        public static void Main(string[] args)
        {
			try
			{
				var task = ParseCommandLine(Environment.CommandLine);

				if (task.IsNullOrEmpty())
				{
					Trace.Write("Failed to parse task from command line");
					return;
				}

				string ravenInstanceId = Environment.GetEnvironmentVariable("raveninstanceid");
				if (ravenInstanceId.IsNullOrEmpty())
				{
					Trace.Write("No raven instance set, defaulting to instance #1");
					ravenInstanceId = "1";
				}

				ObjectFactory.Container.Install(new TasksMasterInstaller());
				ObjectFactory.GetObject<ITask>(task).Execute(ravenInstanceId);
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
		private static string ParseCommandLine(string commandLine)
		{
			var parser = new StringCommandLineParser();
			var result = parser.All(commandLine);

			while (result != null)
			{
				var element = result.Value as DefinitionElement;

				if (element != null && element.Key.ToLowerInvariant() == "task")
				{
					return element.Value.ToLowerInvariant().Trim();
				}

				result = parser.All(result.Rest);
			}

			return null;
		}
    }
}
