using System;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class ShellWrapper
		: IShell
	{
		private readonly IShell _shell;
		private readonly string _machine;

		public ShellWrapper(IShell shell, string machine)
		{
			_shell = shell;
			_machine = machine;
		}

		#region Implementation of IShell

		public ProcessOutput StartAndWaitForExit(string file, string commandLine, TimeSpan timeout,
			bool printStdOutOnFailure, bool showWindow)
		{
			try
			{
				return _shell.StartAndWaitForExit(file, commandLine, timeout, printStdOutOnFailure, showWindow);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public int ExecuteCommand(string command, bool showWindow)
		{
			try
			{
				return _shell.ExecuteCommand(command, showWindow);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		#endregion
	}
}