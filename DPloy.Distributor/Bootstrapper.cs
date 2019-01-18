using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using csscript;
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
				AssemblyLoader.LoadAssembliesFrom(Assembly.GetExecutingAssembly(), "Dependencies");
				return Run(args);
			}
			catch (Exception e)
			{
				Console.WriteLine("Terminating due to unexpected exception:");
				Console.WriteLine(e);
				return -2;
			}
		}

		private static int Run(string[] args)
		{
			Log4Net.Setup(Constants.Distributor, args);

			return Parser.Default.ParseArguments<DeployOptions, ExampleOptions>(args)
				.MapResult(
					(DeployOptions options) => RunDeployment(options),
					(ExampleOptions options) => ShowExample(options),
					errs => 1);
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

			return 0;
		}

		private static int RunDeployment(DeployOptions options)
		{
			var scriptPath = Paths.NormalizeAndEvaluate(options.Script);
			var arguments = options.ScriptArguments.ToArray();
			try
			{
				return ScriptRunner.Run(scriptPath, arguments);
			}
			catch (CompilerException e)
			{
				var errors = (List<string>)e.Data["Errors"];
				foreach (string err in errors)
				{
					Console.WriteLine("{0}, {1}", scriptPath, err);
				}
				var warnings = (List<string>)e.Data["Warnings"];
				foreach (string warning in warnings)
				{
					Console.WriteLine("{0}, {1}", scriptPath, warning);
				}
				return -1;
			}
			catch (TargetInvocationException e)
			{
				PrintException(options, e.InnerException);
				return -1;
			}
			catch (Exception e)
			{
				PrintException(options, e);
				return -1;
			}
		}

		private static void PrintException(DeployOptions options, Exception e)
		{
			if (options.Verbose)
			{
				Console.WriteLine("Error:\r\n{0}", e);
			}
			else
			{
				Console.WriteLine("Error: {0}", e.Message);
			}
		}
	}
}