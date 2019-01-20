using System;
using System.Reflection;
using CommandLine;
using DPloy.Core;

namespace DPloy.Node
{
	internal static class Bootstrapper
	{
		private static int Main(string[] args)
		{
			try
			{
				AssemblyLoader.LoadAssembliesFrom(Assembly.GetExecutingAssembly(), "Dependencies");
				return Run(args);
			}
			catch (Exception e)
			{
				PrintFatalException(e);
				return (int)ExitCode.UnknownError;
			}
		}

		private static int Run(string[] args)
		{
			Log4Net.Setup(Constants.Node, args, logToConsole: true);

			return Parser.Default.ParseArguments<CommandLineOptions>(args)
				.MapResult(
					(CommandLineOptions options) => Application.Run(options),
					errs => 1);
		}

		private static void PrintFatalException(Exception e)
		{
			Console.WriteLine("Terminating due to unexpected exception:");
			Console.WriteLine(e);
		}
	}
}