using CommandLine;

namespace DPloy.Distributor
{
	internal sealed class CommandLineOptions
	{
		[Option('s', "script", Required = true, HelpText = "The deployment cs-script to execute.")]
		public string Script { get; set; }

		[Option('v', "verbose", HelpText = "Set output to verbose messages (this includes stacktraces)")]
		public bool Verbose { get; set; }
	}
}