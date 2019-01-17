using DPloy.Core;

namespace DPloy.Distributor
{
	class Application
	{
		public static void Run(CommandLineOptions opts)
		{
			Executor.Run(Paths.NormalizeAndEvaluate(opts.Script));
		}
	}
}
