using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
		private const string RunEntryPointName = "Run";
		private const string DeployEntryPointName = "Deploy";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="consoleWriter"></param>
		/// <param name="scriptFilePath"></param>
		/// <returns></returns>
		public static ExitCode ListEntryPoints(IOperationTracker consoleWriter, string scriptFilePath)
		{
			var script = LoadAndCompileScript(consoleWriter, scriptFilePath);
			var runMethods = FindMethods(script, RunEntryPointName);
			var deployMethods = FindMethods(script, DeployEntryPointName);

			if (runMethods.Any())
			{
				Console.WriteLine("Found {0} 'run' entry points:", runMethods.Count);
				foreach (var method in runMethods)
				{
					Console.WriteLine("\t{0}", FormatMethod(method));
				}
			}

			if (deployMethods.Any())
			{
				Console.WriteLine("Found {0} 'deploy' entry points:", deployMethods.Count);
				foreach (var method in deployMethods)
				{
					Console.WriteLine("\t{0}", FormatMethod(method));
				}
			}

			if (runMethods.Count == 0 && deployMethods.Count == 0)
			{
				Console.WriteLine("The script doesn't contain any entry point");
			}

			return ExitCode.Success;
		}

		/// <summary>
		/// Executes the given script's Run(string[]) method.
		/// </summary>
		/// <param name="operationTracker"></param>
		/// <param name="scriptFilePath"></param>
		/// <param name="scriptArguments"></param>
		/// <returns></returns>
		public static int Run(IOperationTracker operationTracker, string scriptFilePath, IReadOnlyList<string> scriptArguments)
		{
			var script = LoadAndCompileScript(operationTracker, scriptFilePath);
			Log.InfoFormat("Executing '{0}'...", scriptFilePath);

			var exitCode = RunPrivate(operationTracker, script, scriptArguments);

			Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

			return exitCode;
		}

		/// <summary>
		/// Executes the given script's Deploy(INode) method.
		/// </summary>
		/// <param name="operationTracker"></param>
		/// <param name="scriptFilePath"></param>
		/// <param name="nodeAddresses"></param>
		/// <param name="arguments"></param>
		/// <param name="connectTimeout"></param>
		/// <returns></returns>
		public static int Deploy(IOperationTracker operationTracker,
		                         string scriptFilePath,
		                         IReadOnlyList<string> nodeAddresses,
		                         IEnumerable<string> arguments,
		                         TimeSpan connectTimeout)
		{
			var script = LoadAndCompileScript(operationTracker, scriptFilePath);
			Log.InfoFormat("Executing '{0}'...", scriptFilePath);

			var exitCode = Deploy(script, operationTracker, nodeAddresses, arguments, connectTimeout);

			Log.InfoFormat("'{0}' returned '{1}'", scriptFilePath, exitCode);

			return exitCode;
		}

		private static int RunPrivate(IOperationTracker operationTracker, object script, IReadOnlyList<string> args)
		{
			var method = FindMethod(script, RunEntryPointName);
			if (method == null)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			using (var taskScheduler = new DefaultTaskScheduler())
			{
				var filesystem = new Filesystem(taskScheduler);
				using (var node = new LocalNode(operationTracker, filesystem))
				{
					var parameters = method.GetParameters();
					var scriptArguments = new List<object>();
					if (parameters.Length > 1 && parameters[0].ParameterType == typeof(INode))
						scriptArguments.Add(node);
					scriptArguments.Add(args);

					if (method.ReturnType == typeof(void))
					{
						InvokeMethod(script, method, scriptArguments);
						return 0;
					}

					if (method.ReturnType == typeof(int))
					{
						return (int)InvokeMethod(script, method, scriptArguments);
					}
				}
			}

			throw new ScriptExecutionException($"Expected main entry point to either return no value or to return an Int32, but found: {method.ReturnType.Name}");
		}

		private static int Deploy(object script,
		                          IOperationTracker operationTracker,
		                          IReadOnlyList<string> nodeAddresses,
		                          IEnumerable<string> arguments,
		                          TimeSpan connectTimeout)
		{
			var method = FindDeployMethod(script);

			if (nodeAddresses.Count == 1)
			{
				return DeployTo(script, method, operationTracker, nodeAddresses[0], arguments, connectTimeout);
			}

			var tracker = new NodeTracker(operationTracker, nodeAddresses);
			Parallel.ForEach(nodeAddresses, nodeAddress =>
			{
				var nodeTracker = tracker.Get(nodeAddress);
				try
				{
					DeployTo(script, method, nodeTracker, nodeAddress, arguments, connectTimeout);
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

			return 0;
		}

		private static MethodInfo FindDeployMethod(object script)
		{
			const string expectedSignature = "void Deploy(INode)";

			var method = FindMethod(script, DeployEntryPointName);
			if (method == null)
				throw new ScriptExecutionException($"The script is missing a '{expectedSignature}' entry point");

			if (!HasDeploySignature(method))
				throw new
					ScriptExecutionException($"Expected an entry with the following signature '{expectedSignature}' but '{method.Name}' has an incompatible signature!");

			Log.DebugFormat("Using entry point '{0}'", method);

			return method;
		}

		private static bool HasDeploySignature(MethodInfo method)
		{
			var signatures = new List<MethodSignature>
			{
				new MethodSignature
				{
					ReturnType = typeof(void),
					ParameterTypes = new[] {typeof(INode)}
				},
				new MethodSignature
				{
					ReturnType = typeof(void),
					ParameterTypes = new[] {typeof(INode), typeof(string[])}
				},
				new MethodSignature
				{
					ReturnType = typeof(void),
					ParameterTypes = new[] {typeof(INode), typeof(INode)}
				},
				new MethodSignature
				{
					ReturnType = typeof(void),
					ParameterTypes = new[] {typeof(INode), typeof(INode), typeof(string[])}
				}
			};
			signatures.AddRange(signatures.ToList().Select(x => x.WithReturnType(typeof(int))));
			return signatures.Any(x => x.IsCompatibleTo(method));
		}

		private static int DeployTo(object script,
		                             MethodInfo method,
		                             IOperationTracker operationTracker,
		                             string nodeAddress,
		                             IEnumerable<string> arguments,
		                             TimeSpan connectTimeout)
		{
			Log.InfoFormat("Executing '{0}' for '{1}'", method, nodeAddress);

			using (var taskScheduler = new DefaultTaskScheduler())
			{
				var filesystem = new Filesystem(taskScheduler);
				using (var distributor = new Distributor(operationTracker))
				using (var localNode = new LocalNode(operationTracker, filesystem))
				using (var remoteNode = distributor.ConnectTo(nodeAddress, connectTimeout))
				{
					var args = new List<object>();
					var parameters = method.GetParameters();
					if (parameters[0].ParameterType == typeof(INode) && parameters.Length >= 2 &&
					    parameters[1].ParameterType == typeof(INode))
						args.Add(localNode);
					args.Add(remoteNode);
					if (parameters[parameters.Length - 1].ParameterType == typeof(string[]))
						args.Add(arguments);

					var ret = InvokeMethod(script, method, args.ToArray());
					if (method.ReturnType == typeof(int))
						return (int)ret;

					return 0;
				}
			}
		}

		[Pure]
		private static MethodInfo FindMethod(object script, string methodName)
		{
			var methods = FindMethods(script, methodName);
			if (methods.Count == 0)
				throw new ScriptExecutionException($"The script is missing a main entry point");

			if (methods.Count > 1)
				throw new ScriptExecutionException($"The script contains too many entry points: There may only be one Main() method!");

			return methods[0];
		}

		[Pure]
		private static IReadOnlyList<MethodInfo> FindMethods(object script, string methodName)
		{
			var scriptType = script.GetType();
			var methods = scriptType.GetMethods()
			                        .Concat(scriptType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
			                        .Concat(scriptType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
			                        .Where(x => x.Name == methodName).ToList();
			return methods;
		}

		private static object InvokeMethod(object scriptObject, MethodInfo main, IReadOnlyList<object> scriptArguments)
		{
			object obj = main.IsStatic ? null : scriptObject;

			try
			{
				return main.Invoke(obj, scriptArguments.ToArray());
			}
			catch (TargetInvocationException e)
			{
				var inner = e.InnerException;
				if (inner != null)
					throw new ScriptExecutionException(inner.Message, inner);

				throw;
			}
		}

		private static object LoadAndCompileScript(IOperationTracker operationTracker, string scriptFilePath)
		{
			var script = LoadAndPreprocessScript(operationTracker, scriptFilePath);
			return CompileScript(operationTracker, scriptFilePath, script);
		}

		private static string LoadAndPreprocessScript(IOperationTracker operationTracker, string scriptFilePath)
		{
			Log.InfoFormat("Loading '{0}'...", scriptFilePath);

			var operation = operationTracker.BeginLoadScript(scriptFilePath);

			string script;
			try
			{
				using (var taskScheduler = new DefaultTaskScheduler())
				{
					var filesystem = new Filesystem(taskScheduler);
					var processor = new ScriptPreprocessor(filesystem);
					script = processor.ProcessFile(scriptFilePath, new string[0]);
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

		private static object CompileScript(IOperationTracker operationTracker, string scriptFilePath, string script)
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
				var tmp = new ScriptCompilationException(scriptFilePath, e);
				operation.Failed(e);
				throw tmp;
			}
		}

		[Pure]
		private static string FormatMethod(MethodInfo method)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("{0} {1}.{2}(",
			                     method.ReturnType.Name,
			                     method.DeclaringType.FullName,
			                     method.Name);
			var parameters = method.GetParameters();
			for (int i = 0; i < parameters.Length; ++i)
			{
				if (i > 0)
					builder.Append(", ");

				var parameter = parameters[i];
				builder.AppendFormat("{0} {1}", parameter.ParameterType.FullName, parameter.Name);
			}

			builder.Append(")");

			return builder.ToString();
		}
	}
}