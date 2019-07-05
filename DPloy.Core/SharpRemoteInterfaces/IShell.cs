using System;
using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface IShell
	{
		/// <summary>
		///    Starts a new process and waits for it to exit.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="commandLine"></param>
		/// <param name="timeout">The maximum amount of time to wait for the process to exit, -1ms is interpreted as an infinite amount of time</param>
		/// <param name="printStdOutOnFailure"></param>
		/// <param name="showWindow"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		ProcessOutput StartAndWaitForExit(string file, string commandLine, TimeSpan timeout, bool printStdOutOnFailure, bool showWindow);

		[Invoke(Dispatch.SerializePerObject)]
		int ExecuteCommand(string command, bool showWindow);
	}
}