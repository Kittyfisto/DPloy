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

			return Parser.Default.ParseArguments<DeployOptions, RunOptions, ExampleOptions>(args)
			             .MapResult(
			                        (DeployOptions options) => (int) Deploy(options),
			                        (RunOptions options) => (int)Run(options),
			                        (ExampleOptions options) => ShowExample(options),
			                        errs => (int) ExitCode.InvalidArguments);
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

			var arguments = options.ScriptArguments.ToArray();
			try
			{
				var progressWriter = new ConsoleWriter(options.Verbose);
				return (ExitCode) ScriptRunner.Run(progressWriter, scriptPath, arguments);
			}
			catch (ScriptCannotBeAccessedException e)
			{
				Console.WriteLine();
				Console.WriteLine("Error: {0}", e.Message);
				return ExitCode.ScriptCannotBeAccessed;
			}
			catch (ScriptCompilationException e)
			{
				Console.WriteLine();
				Console.WriteLine("Build failed:");

				foreach (var err in e.Errors) Console.WriteLine("{0}, {1}", scriptPath, err);
				foreach (var warning in e.Warnings) Console.WriteLine("{0}, {1}", scriptPath, warning);
				return ExitCode.CompileError;
			}
			catch (ScriptExecutionException e)
			{
				Console.WriteLine();
				PrintUnhandledScriptException(options.Verbose, e);
				return ExitCode.ExecutionError;
			}
			catch (Exception e)
			{
				Console.WriteLine();
				PrintUnhandledException(e);
				return ExitCode.UnhandledException;
			}
		}

		private static ExitCode Deploy(DeployOptions options)
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

			var nodes = options.Nodes.ToArray();
			try
			{
				var progressWriter = new ConsoleWriter(options.Verbose);
				return (ExitCode) ScriptRunner.Deploy(progressWriter, scriptPath, nodes);
			}
			catch (ScriptCannotBeAccessedException e)
			{
				Console.WriteLine();
				Console.WriteLine("Error: {0}", e.Message);
				return ExitCode.ScriptCannotBeAccessed;
			}
			catch (ScriptCompilationException e)
			{
				Console.WriteLine();
				Console.WriteLine("Build failed:");

				foreach (var err in e.Errors) Console.WriteLine("{0}, {1}", scriptPath, err);
				foreach (var warning in e.Warnings) Console.WriteLine("{0}, {1}", scriptPath, warning);
				return ExitCode.CompileError;
			}
			catch (ScriptExecutionException e)
			{
				Console.WriteLine();
				PrintUnhandledScriptException(options.Verbose, e);
				return ExitCode.ExecutionError;
			}
			catch (Exception e)
			{
				Console.WriteLine();
				PrintUnhandledException(e);
				return ExitCode.UnhandledException;
			}
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