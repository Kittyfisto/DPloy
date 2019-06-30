using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using DPloy.Core;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Options;
using DPloy.Distributor.Output;

namespace DPloy.Distributor
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
				PrintUnhandledException(e);
				return (int) ExitCode.UnhandledException;
			}
		}

		private static int Run(string[] args)
		{
			Log4Net.Setup(Constants.Distributor, args);

			try
			{
				var result = Parser.Default.ParseArguments<DeployOptions, RunOptions, ListOptions, ExampleOptions>(args);
				return result.MapResult(
				                        (DeployOptions options) => (int) Deploy(options),
				                        (RunOptions options) => (int)Run(options),
				                        (ListOptions options) => (int)Run(options),
				                        (ExampleOptions options) => ShowExample(options),
				                        errs => (int) ExitCode.InvalidArguments);
			}
			catch (ScriptCannotBeAccessedException e)
			{
				Console.WriteLine();
				Console.WriteLine("Error: {0}", e.Message);
				return (int) ExitCode.ScriptCannotBeAccessed;
			}
			catch (ScriptCompilationException e)
			{
				Console.WriteLine();
				Console.WriteLine("Build failed:");

				foreach (var err in e.Errors) Console.WriteLine("{0}, {1}", e.ScriptPath, err);
				foreach (var warning in e.Warnings) Console.WriteLine("{0}, {1}", e.ScriptPath, warning);
				return (int) ExitCode.CompileError;
			}
			catch (ScriptExecutionException e)
			{
				Console.WriteLine();
				PrintUnhandledScriptException(false, e);
				return (int) ExitCode.ExecutionError;
			}
			catch (NotConnectedException)
			{
				Console.WriteLine();
				Console.WriteLine("Terminating the application because a node could not be connected to");

				return (int) ExitCode.ConnectionError;
			}
			catch (Exception e)
			{
				Console.WriteLine();
				PrintUnhandledException(e);
				return (int) ExitCode.UnhandledException;
			}
		}

		private static int ShowExample(ExampleOptions options)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var exampleScriptName = assembly.GetManifestResourceNames()
			                                .First(x => x.Contains("HelloWorld.cs"));
			using (var stream = assembly.GetManifestResourceStream(exampleScriptName))
			using (var reader = new StreamReader(stream))
			{
				var exampleScript = reader.ReadToEnd();
				Console.WriteLine(exampleScript);
			}

			return (int) ExitCode.Success;
		}

		private static ExitCode Run(RunOptions options)
		{
			var scriptPath = NormalizeAndEvaluate(options.Script);
			var arguments = options.ScriptArguments.ToArray();
			var progressWriter = new ConsoleWriter(options.Verbose);
			return (ExitCode) ScriptRunner.Run(progressWriter, scriptPath, arguments);
		}

		private static ExitCode Deploy(DeployOptions options)
		{
			var scriptPath = NormalizeAndEvaluate(options.Script);

			var progressWriter = new ConsoleWriter(options.Verbose);
			var nodes = options.Nodes.ToArray();
			return (ExitCode) ScriptRunner.Deploy(progressWriter,
			                                      scriptPath,
			                                      nodes,
			                                      options.Arguments,
			                                      TimeSpan.FromSeconds(options.TimeoutInSeconds));
		}

		private static string NormalizeAndEvaluate(string scriptPath)
		{
			try
			{
				return Paths.NormalizeAndEvaluate(scriptPath);
			}
			catch (IOException e)
			{
				throw new ScriptCannotBeAccessedException(e.Message, null);
			}
		}

		private static ExitCode Run(ListOptions options)
		{
			string scriptPath;
			try
			{
				scriptPath = Paths.NormalizeAndEvaluate(options.Script);
			}
			catch (IOException e)
			{
				Console.WriteLine("Error: {0}", e.Message);
				return ExitCode.ScriptCannotBeAccessed;
			}
			
			var progressWriter = new ConsoleWriter(options.Verbose);
			return ScriptRunner.ListEntryPoints(progressWriter, scriptPath);
		}

		private static void PrintUnhandledScriptException(bool verbose, ScriptExecutionException e)
		{
			Console.WriteLine("Terminating due to unhandled exception in the script file:");
			PrintException(verbose, e);
		}

		private static void PrintUnhandledException(Exception e)
		{
			Console.WriteLine("Terminating due to an unhandled exception:");
			Console.WriteLine(e);
		}

		private static void PrintException(bool verbose, Exception e)
		{
			if (verbose)
				Console.WriteLine("\tError:\r\n{0}", e);
			else
				Console.WriteLine("\tError: {0}", e.Message);
		}
	}
}