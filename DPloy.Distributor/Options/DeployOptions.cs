using System.Collections.Generic;
using CommandLine;

namespace DPloy.Distributor.Options
{
	[Verb("deploy", HelpText = "Execute a deployment cs-script to deploy software/files to one or more remote machines")]
	internal sealed class DeployOptions
	{
		[Value(0, MetaName = "Script", Required = true, HelpText = "The deployment cs-script to execute.")]
		public string Script { get; set; }

		[Value(1, MetaName = "Nodes", Required = true, HelpText = "The list of nodes to which software shall be deployed. Can be a list of computer names, IP-Addresses, etc...")]
		public IEnumerable<string> Nodes { get; set; }

		[Option('v', "verbose")]
		public bool Verbose { get; set; }

		[Option('a', "arguments")]
		public IEnumerable<string> Arguments { get; set; }
	}
}