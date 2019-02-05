using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using csscript;
using CSScriptLibrary;
using DPloy.Core.PublicApi;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Output;
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
			Log.InfoFormat("Executing '{0}'...", scriptFilePath);

			var exitCode = Run(script, scriptArguments);

			Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

			return exitCode;
		}

		/// <summary>
		/// Executes the given script's Deploy(INode) method.
		/// </summary>
		/// <param name="consoleWriter"></param>
		/// <param name="scriptFilePath"></param>
		/// <param name="nodeAddresses"></param>
		/// <returns></returns>
		public static int Deploy(ConsoleWriter consoleWriter, string scriptFilePath, IReadOnlyList<string> nodeAddresses)
		{
			var script = LoadAndCompileScript(consoleWriter, scriptFilePath);
			Log.InfoFormat("Executing '{0}'...", scriptFilePath);

			var exitCode = Deploy(script, consoleWriter, nodeAddresses);

			Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

			return exitCode;
		}

		private static int Run(object script, IReadOnlyList<string> args)
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

		private static int Deploy(object script, ConsoleWriter consoleWriter, IReadOnlyList<string> nodeAddresses)
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
				if (nodeAddresses.Count == 1)
				{
					DeployTo(script, method, consoleWriter, nodeAddresses[0]);
				}
				else
				{
					var tracker = new NodeTracker(consoleWriter, nodeAddresses);
					Parallel.ForEach(nodeAddresses, nodeAddress =>
					{
						var nodeTracker = tracker.Get(nodeAddress);
						try
						{
							DeployTo(script, method, nodeTracker, nodeAddress);
							nodeTracker.Success();
						}
						catch (Exception e)
						{
							// We want keep track of failures on individual nodes
							// and only rethrow those exceptions after everything's done.
							nodeTracker.Failed(e);
						}
					});

					tracker.ThrowOnFailure();
				}

				
				return 0;
			}

			throw new ScriptExecutionException($"Expected an entry with the following signature '{expectedSignature}' but '{method.ReturnType.Name}' has an incompatible signature!");
		}

		private static void DeployTo(object script, MethodInfo method, IOperationTracker operationTracker, string nodeAddress)
		{
			using (var distributor = new Distributor(operationTracker))
			using (var node = distributor.ConnectTo(nodeAddress))
			{
				InvokeMethod(script, method, new object[] {node});
			}
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

		private static object LoadAndCompileScript(ConsoleWriter operationTracker, string scriptFilePath)
		{
			var script = LoadAndPreprocessScript(operationTracker, scriptFilePath);
			return CompileScript(operationTracker, scriptFilePath, script);
		}

		private static string LoadAndPreprocessScript(ConsoleWriter operationTracker, string scriptFilePath)
		{
			Log.InfoFormat("Loading '{0}'...", scriptFilePath);

			var operation = operationTracker.BeginLoadScript(scriptFilePath);

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

		private static object CompileScript(ConsoleWriter operationTracker, string scriptFilePath, string script)
		{
			Log.InfoFormat("Compiling '{0}'...", scriptFilePath);

			var evaluator = CSScript.Evaluator;
			evaluator.ReferenceAssembly(typeof(INode).Assembly);

			var operation = operationTracker.BeginCompileScript(scriptFilePath);

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