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

		public int ExecuteFile(string file, string commandLine)
		{
			try
			{
				return _shell.ExecuteFile(file, commandLine);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public int ExecuteCommand(string command)
		{
			try
			{
				return _shell.ExecuteCommand(command);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		#endregion
	}
}