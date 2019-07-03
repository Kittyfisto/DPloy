using System.Diagnostics;
using System.Reflection;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Core.SharpRemoteImplementations
{
	public sealed class Processes
		: IProcesses
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int KillAll(string[] names)
		{
			int numKilled = 0;
			foreach (var name in names)
			{
				numKilled += KillAll(name);
			}
			return numKilled;
		}

		private static int KillAll(string name)
		{
			Log.DebugFormat("Kill all processes named '{0}'...", name);

			var processes = Process.GetProcessesByName(name);
			foreach (var process in processes)
			{
				process.Kill();
			}

			Log.InfoFormat("Killed {0} process(es) named '{1}'", processes.Length, name);
			return processes.Length;
		}
	}
}
