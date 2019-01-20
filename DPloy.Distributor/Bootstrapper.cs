using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using DPloy.Core;
using DPloy.Distributor.Exceptions;

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

			return Parser.Default.ParseArguments<DeployOptions, ExampleOptions>(args)
			             .MapResult(
			                        (DeployOptions options) => (int) RunDeployment(options),
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

		private static ExitCode RunDeployment(DeployOptions options)
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
				var progressWriter = new ProgressWriter(options.Verbose);
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
				PrintUnhandledScriptException(options, e);
				return ExitCode.ExecutionError;
			}
			catch (Exception e)
			{
				Console.WriteLine();
				PrintUnhandledException(e);
				return ExitCode.UnhandledException;
			}
		}

		private static void PrintUnhandledScriptException(DeployOptions options, ScriptExecutionException e)
		{
			Console.WriteLine("Terminating due to unhandled exception in the script file:");
			PrintException(options, e);
		}

		private static void PrintUnhandledException(Exception e)
		{
			Console.WriteLine("Terminating due to an unhandled exception:");
			Console.WriteLine(e);
		}

		private static void PrintException(DeployOptions options, Exception e)
		{
			if (options.Verbose)
				Console.WriteLine("\tError:\r\n{0}", e);
			else
				Console.WriteLine("\tError: {0}", e.Message);
		}
	}
}