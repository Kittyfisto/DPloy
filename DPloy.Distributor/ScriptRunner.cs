using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using csscript;
using CSScriptLibrary;
using DPloy.Core;
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

		/// <summary>
		/// Executes the given script's Run(string[]) method.
		/// </summary>
		/// <param name="consoleWriter"></param>
		/// <param name="scriptFilePath"></param>
		/// <param name="scriptArguments"></param>
		/// <returns></returns>
		public static int Run(ConsoleWriter consoleWriter, string scriptFilePath, IReadOnlyList<string> scriptArguments)
		{
			var script = LoadAndCompileScript(consoleWriter, scriptFilePath);
			using (var distributor = new Distributor(consoleWriter))
			{
				Log.InfoFormat("Executing '{0}'...", scriptFilePath);

				var exitCode = Run(script, distributor, scriptArguments);

				Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

				return exitCode;
			}
		}

		/// <summary>
		/// Executes the given script's Deploy(INode) method.
		/// </summary>
		/// <param name="consoleWriter"></param>
		/// <param name="scriptFilePath"></param>
		/// <param name="nodes"></param>
		/// <returns></returns>
		public static int Deploy(ConsoleWriter consoleWriter, string scriptFilePath, IReadOnlyList<string> nodes)
		{
			var script = LoadAndCompileScript(consoleWriter, scriptFilePath);
			using (var distributor = new Distributor(consoleWriter))
			{
				Log.InfoFormat("Executing '{0}'...", scriptFilePath);

				var exitCode = Deploy(script, distributor, nodes);

				Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

				return exitCode;
			}
		}

		private static int Run(object script, Distributor distributor, IReadOnlyList<string> args)
		{
			var method = FindMethod(script, "Run");
			if (method == null)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			if (method.ReturnType == typeof(void))
			{
				InvokeMethod(script, method, new object[]{args});
				return 0;
			}

			if (method.ReturnType == typeof(int))
			{
				return (int) InvokeMethod(script, method, new object[]{args});
			}

			throw new ScriptExecutionException($"Expected main entry point to either return no value or to return an Int32, but found: {method.ReturnType.Name}");
		}

		private static int Deploy(object script, Distributor distributor, IReadOnlyList<string> nodes)
		{
			const string expectedSignature = "void Deploy(INode)";

			var method = FindMethod(script, "Deploy");
			if (method == null)
				throw new ScriptExecutionException($"The script is missing a '{expectedSignature}' entry point");

			var parameters = method.GetParameters();
			if (method.ReturnType == typeof(void) &&
			    parameters.Length == 1 &&
			    parameters[0].ParameterType == typeof(INode) &&
			    !parameters[0].IsRetval && 
			    !parameters[0].IsOut)
			{
				foreach (var nodeAddress in nodes)
				{
					using (var node = distributor.ConnectTo(nodeAddress))
					{
						InvokeMethod(script, method, new object[]{node});
					}
				}
				return 0;
			}

			throw new ScriptExecutionException($"Expected an entry with the following signature '{expectedSignature}' but '{method.ReturnType.Name}' has an incompatible signature!");
		}

		[Pure]
		private static MethodInfo FindMethod(object script, string methodName)
		{
			var scriptType = script.GetType();
			var methods = scriptType.GetMethods()
				.Concat(scriptType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
				.Concat(scriptType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
				.Where(x => x.Name == methodName).ToList();

			if (methods.Count == 0)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			if (methods.Count > 1)
				throw new ScriptExecutionException($"The script contains too many entry points: There may only be one Main() method!");

			return methods[0];
		}

		private static object InvokeMethod(object scriptObject, MethodInfo main, object[] scriptArguments)
		{
			object obj = main.IsStatic ? null : scriptObject;

			try
			{
				return main.Invoke(obj, scriptArguments);
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
			evaluator.ReferenceAssembly(typeof(INode).Assembly);

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