using System.Collections.Generic;
using CommandLine;

namespace DPloy.Node
{
	internal sealed class CommandLineOptions
	{
		[Option('w', "whitelist", Required = true, HelpText = "A list of hostnames which are allowed to remotely deploy software and execute commands")]
		public IEnumerable<string> AllowedHosts { get; set; }
	}
}
