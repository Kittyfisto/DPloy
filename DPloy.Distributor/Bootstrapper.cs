using System;
using CommandLine;
using DPloy.Core;

namespace DPloy.Distributor
{
	static class Bootstrapper
	{
		static int Main(string[] args)
		{
			try
			{
				Log4Net.Setup(Constants.Distributor, args);

				int exitCode = 0;
				Parser.Default.ParseArguments<CommandLineOptions>(args)
				      .WithParsed(options =>
				      {
					      exitCode = Run(options);
				      });

				return exitCode;
			}
			catch (Exception e)
			{
				Console.WriteLine("Terminating due to unexpected exception:");
				Console.WriteLine(e);
				return -2;
			}
		}

		private static int Run(CommandLineOptions options)
		{
			try
			{
				Application.Run(options);
				return 0;
			}
			catch (Exception e)
			{
				if (options.Verbose)
				{
					Console.WriteLine("Error:\r\n{0}", e);
				}
				else
				{
					Console.WriteLine("Error: {0}", e.Message);
				}

				return -1;
			}
		}
	}
}