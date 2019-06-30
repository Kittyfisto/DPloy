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
		/// <param name="timeout"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		int ExecuteProcess(string file, string commandLine, TimeSpan timeout);

		[Invoke(Dispatch.SerializePerObject)]
		int ExecuteCommand(string command);
	}
}