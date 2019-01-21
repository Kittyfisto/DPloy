using System.Diagnostics;
using DPloy.Core.SharpRemoteInterfaces;

namespace DPloy.Node.SharpRemoteImplementations
{
	sealed class Processes
		: IProcesses
	{
		public void KillAll(string name)
		{
			var processes = Process.GetProcessesByName(name);
			foreach (var process in processes)
			{
				process.Kill();
			}
		}
	}
}
