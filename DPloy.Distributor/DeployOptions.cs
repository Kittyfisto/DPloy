using System.Collections.Generic;
using CommandLine;

namespace DPloy.Distributor
{
	[Verb("deploy", HelpText = "Execute a deployment cs-script to deploy software/files to one or more remote machines")]
	internal sealed class DeployOptions
	{
		[Option('s', "script", Required = true, HelpText = "The deployment cs-script to execute.")]
		public string Script { get; set; }

		[Option('v', "verbose", HelpText = "Set output to verbose messages (this includes stacktraces)")]
		public bool Verbose { get; set; }

		[Option('a', "scriptarguments", HelpText = "An optional list of arguments to pass to the script")]
		public IEnumerable<string> ScriptArguments { get; set; }
	}
}