using CommandLine;

namespace DPloy.Distributor.Options
{
	[Verb("list", HelpText = "Lists the entry points of a given script")]
	public sealed class ListOptions
	{
		[Value(0, MetaName = "Script", Required = true, HelpText = "The cs-script for which to list the entry points.")]
		public string Script { get; set; }

		[Option('v', "verbose")]
		public bool Verbose { get; set; }
	}
}
