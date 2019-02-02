using System.Collections.Generic;
using CommandLine;

namespace DPloy.Distributor.Options
{
	[Verb("run", HelpText = "Executes a cs-script to do whatever you want")]
	internal sealed class RunOptions
	{
		[Value(0, MetaName = "Script", Required = true, HelpText = "The cs-script to execute.")]
		public string Script { get; set; }

		[Value(1, MetaName = "Arguments", HelpText = "An optional list of arguments which is forwarded to the script's Run(string[]) method")]
		public IEnumerable<string> ScriptArguments { get; set; }

		[Option('v', "verbose")]
		public bool Verbose { get; set; }
	}
}
