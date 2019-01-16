using System;
using System.Net;

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
			using (var node = new Node())
			{
				node.Bind(new IPEndPoint(IPAddress.Loopback, port: 12345));
				WaitUntilExit();
			}
		}

		private static void WaitUntilExit()
		{
			Console.WriteLine("Write exit to end the program...");
			while (true)
			{
				var command = Console.ReadLine();
				if (command == "exit") break;

				Console.WriteLine("Unknown command: {0}", command);
			}
		}
	}
}