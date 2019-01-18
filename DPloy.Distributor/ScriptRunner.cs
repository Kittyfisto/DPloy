using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using csscript;
using CSScriptLibrary;
using DPloy.Core.PublicApi;
using DPloy.Distributor.Exceptions;
using log4net;

namespace DPloy.Distributor
{
	/// <summary>
	///     Responsible for executing deployment scripts.
	/// </summary>
	internal static class ScriptRunner
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static int Run(string scriptFilePath, string[] scriptArguments)
		{
			var script = CompileScript(scriptFilePath);
			using (var distributor = new Distributor())
			{
				Log.InfoFormat("Executing '{0}'...", scriptFilePath);

				var exitCode = RunMain(script, distributor, scriptArguments);

				Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

				return exitCode;
			}
		}

		private static int RunMain(object script, Distributor distributor, string[] args)
		{
			var method = FindEntryPoint(script);
			if (method == null)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			if (method.ReturnType == typeof(void))
			{
				InvokeMethod(script, method, distributor, args);
				return 0;
			}

			if (method.ReturnType == typeof(int))
			{
				return (int) InvokeMethod(script, method, distributor, args);
			}

			throw new ScriptExecutionException($"Expected main entry point to either return no value or to return an Int32, but found: {method.ReturnType.Name}");
		}

		[Pure]
		private static MethodInfo FindEntryPoint(object script)
		{
			var scriptType = script.GetType();
			var methods = scriptType.GetMethods()
				.Concat(scriptType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(scriptType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
				.Where(x => x.Name == "Main").ToList();

			if (methods.Count == 0)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			if (methods.Count > 1)
				throw new ScriptExecutionException($"The script contains too many entry points: There may only be one Main() method!");

			return methods[0];
		}

		private static object InvokeMethod(object scriptObject, MethodInfo main, Distributor distributor, string[] scriptArguments)
		{
			object obj = main.IsStatic ? null : scriptObject;
			var parameters = main.GetParameters();
			var targetType = typeof(IDistributor);
			if (parameters.Length == 0 || parameters[0].ParameterType != targetType)
			{
				throw new ScriptExecutionException($"The script main entry point is must accept one parameter of type {targetType.Name}");
			}

			object[] args;
			if (parameters.Length == 2)
				args = new object[] {distributor, scriptArguments};
			else
				args = new object[] {distributor};

			try
			{
				return main.Invoke(obj, args);
			}
			catch (TargetInvocationException e)
			{
				var inner = e.InnerException;
				if (inner != null)
					throw new ScriptExecutionException(inner.Message, inner);

				throw;
			}
		}

		private static object CompileScript(string scriptFilePath)
		{
			Log.InfoFormat("Compiling '{0}'...", scriptFilePath);

			var script = File.ReadAllText(scriptFilePath);
			var evaluator = CSScript.Evaluator;
			evaluator.ReferenceAssembly(typeof(IDistributor).Assembly);

			try
			{
				var tmp = evaluator.LoadCode(script);
				return tmp;
			}
			catch (CompilerException e)
			{
				throw new ScriptCompilationException(e);
			}
		}
	}
}