using System;
using System.Diagnostics;
using System.Reflection;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Node.SharpRemoteImplementations
{
	internal sealed class Shell
		: IShell
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IShell

		public int Execute(string command)
		{
			using (var process = new Process())
			{
				try
				{
					var startInfo = new ProcessStartInfo
					{
						WindowStyle = ProcessWindowStyle.Hidden,
						FileName = "cmd.exe",
						Arguments = command
					};
					process.StartInfo = startInfo;
					process.Start();
					process.WaitForExit();
				}
				catch (Exception e)
				{
					Log.InfoFormat("Executing '{0}' caused an unexpected exception:\r\n{1}",
					               command,
					               e);
					throw;
				}

				var exitCode = process.ExitCode;
				Log.InfoFormat("Executing '{0}' returned '{1}'", command, exitCode);

				return exitCode;
			}
		}

		#endregion
	}
}