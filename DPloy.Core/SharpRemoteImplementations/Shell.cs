﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Core.SharpRemoteImplementations
{
	public sealed class Shell
		: IShell
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IShell

		public ProcessOutput StartAndWaitForExit(string file, string commandLine, TimeSpan timeout,
			bool printStdOutOnFailure, bool showWindow)
		{
			var fullFilePath = Paths.NormalizeAndEvaluate(file);
			Log.InfoFormat("Executing '{0} {1}'...", fullFilePath, commandLine);

			using (var process = new Process())
			{
				if (!File.Exists(fullFilePath))
					throw new FileNotFoundException($"The path '{fullFilePath}' does not exist or cannot be accessed");

				try
				{
					process.StartInfo = new ProcessStartInfo
					{
						UseShellExecute = true,
						FileName = fullFilePath,
						Arguments = commandLine
					};

					if (!showWindow)
					{
						process.StartInfo.RedirectStandardOutput = true;
						process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						process.StartInfo.UseShellExecute = false;
						process.StartInfo.CreateNoWindow = true;
					}

					var output = StartAndWaitForExit(process, timeout, showWindow ? false : printStdOutOnFailure);
					Log.InfoFormat("The command '{0} {1}' returned '{2}'", fullFilePath, commandLine, output.ExitCode);
					return output;
				}
				catch (Exception e)
				{
					Log.InfoFormat("The command '{0} {1}' caused an unexpected exception:\r\n{2}",
						fullFilePath,
						commandLine,
						e);
					throw;
				}
			}
		}

		private static ProcessOutput StartAndWaitForExit(Process process, TimeSpan timeout, bool printStdOutOnFailure)
		{
			string output = null;

			process.Start();

			var task = Task.Factory.StartNew(() =>
			{
				if (printStdOutOnFailure)
				{
					output = process.StandardOutput.ReadToEnd();
				}
				process.WaitForExit();

				if (printStdOutOnFailure)
				{
					Log.DebugFormat("StandardOutput: {0}", output);
				}
			});

			if (!task.Wait(timeout))
			{
				TryKill(process);

				var message = $"The process failed to exit with {(int) timeout.TotalSeconds}s, killing it";
				throw new TimeoutException(message);
			}

			return new ProcessOutput
			{
				ExitCode = process.ExitCode,
				StandardOutput = output,
			};
		}

		private static void TryKill(Process process)
		{
			try
			{
				process.Kill();
			}
			catch (Exception e)
			{
				Log.WarnFormat("Unable to kill process: {0}", e);
			}
		}

		public int ExecuteCommand(string command, bool showWindow)
		{
			Log.InfoFormat("Executing '{0}'...", command);

			using (var process = new Process())
			{
				try
				{
					var startInfo = new ProcessStartInfo
					{
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = "cmd.exe",
						Arguments = $"/C {command}"
					};
					process.StartInfo = startInfo;
					process.Start();
					process.WaitForExit();
				}
				catch (Exception e)
				{
					Log.InfoFormat("The command '{0}' caused an unexpected exception:\r\n{1}",
						command,
						e);
					throw;
				}

				var exitCode = process.ExitCode;
				Log.InfoFormat("The command '{0}' returned '{1}'", command, exitCode);

				return exitCode;
			}
		}

		#endregion
	}
}