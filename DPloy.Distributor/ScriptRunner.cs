using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

		public static int Run(ConsoleWriter consoleWriter, string scriptFilePath, string[] scriptArguments)
		{
			var script = LoadAndCompileScript(consoleWriter, scriptFilePath);
			using (var distributor = new Distributor(consoleWriter))
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

		private static object LoadAndCompileScript(ConsoleWriter consoleWriter, string scriptFilePath)
		{
			var script = LoadAndPreprocessScript(consoleWriter, scriptFilePath);
			return CompileScript(consoleWriter, scriptFilePath, script);
		}

		private static string LoadAndPreprocessScript(ConsoleWriter consoleWriter, string scriptFilePath)
		{
			Log.InfoFormat("Loading '{0}'...", scriptFilePath);

			var operation = consoleWriter.BeginLoadScript(scriptFilePath);

			string script;
			try
			{
				using (var taskScheduler = new SerialTaskScheduler())
				{
					var filesystem = new Filesystem(taskScheduler);
					var processor = new ScriptPreprocessor(filesystem);
					script = processor.ProcessFileAsync(scriptFilePath, new string[0]).Result;
				}

				operation.Success();
			}
			catch(AggregateException e)
			{
				var exceptions = Unpack(e);
				if (exceptions.Count == 1)
				{
					var exception = exceptions.First();

					operation.Failed(exception);
					throw exception;
				}

				operation.Failed(e);
				throw;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}

			return script;
		}

		private static IReadOnlyList<Exception> Unpack(AggregateException aggregateException)
		{
			var exceptions = new List<Exception>();
			foreach (var exception in aggregateException.InnerExceptions)
			{
				if (exception is AggregateException innerAggregateException)
				{
					exceptions.AddRange(Unpack(innerAggregateException));
				}
				else
				{
					exceptions.Add(exception);
				}
			}

			return exceptions;
		}

		private static object CompileScript(ConsoleWriter consoleWriter, string scriptFilePath, string script)
		{
			Log.InfoFormat("Compiling '{0}'...", scriptFilePath);

			var evaluator = CSScript.Evaluator;
			evaluator.ReferenceAssembly(typeof(IDistributor).Assembly);

			var operation = consoleWriter.BeginCompileScript(scriptFilePath);

			try
			{
				var tmp = evaluator.LoadCode(script);
				operation.Success();
				return tmp;
			}
			catch (CompilerException e)
			{
				var tmp = new ScriptCompilationException(e);
				operation.Failed(e);
				throw tmp;
			}
		}
	}
}