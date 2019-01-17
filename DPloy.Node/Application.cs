using System;
using System.Net;
using SharpRemote.ServiceDiscovery;

namespace DPloy.Node
{
	/// <summary>
	///     Responsible for setting up this node, i.e. establishing
	///     a listening SharpRemote socket and waiting for a incoming connections.
	/// </summary>
	internal class Application
	{
		public static void Run()
		{
			var machineName = Environment.MachineName;
			var serviceName = $"{machineName}.DPloy.Node";

			using (var discoverer = new NetworkServiceDiscoverer())
			using (var node = new NodeServer(serviceName, discoverer))
			{
				node.Bind(IPAddress.Any);

				Console.WriteLine("Waiting for incoming connections (you can write exit to end the program)...");

				WaitUntilExit();
			}
		}

		private static void WaitUntilExit()
		{
			while (true)
			{
				var command = Console.ReadLine();
				if (command == "exit") break;

				Console.WriteLine("Unknown command: {0}", command);
			}
		}
	}
}