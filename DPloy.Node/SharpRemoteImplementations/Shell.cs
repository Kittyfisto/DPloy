﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using DPloy.Core;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Node.SharpRemoteImplementations
{
	internal sealed class Shell
		: IShell
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IShell

		public int ExecuteFile(string file, string commandLine)
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
						RedirectStandardOutput = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						UseShellExecute = false,
						FileName = fullFilePath,
						Arguments = commandLine
					};
					process.Start();

					var output = process.StandardOutput.ReadToEnd();
					process.WaitForExit();
				}
				catch (Exception e)
				{
					Log.InfoFormat("The command '{0} {1}' caused an unexpected exception:\r\n{2}",
						fullFilePath,
						commandLine,
						e);
					throw;
				}

				var exitCode = process.ExitCode;
				Log.InfoFormat("The command '{0} {1}' returned '{2}'", fullFilePath, commandLine, exitCode);

				return exitCode;
			}
		}

		public int ExecuteCommand(string command)
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