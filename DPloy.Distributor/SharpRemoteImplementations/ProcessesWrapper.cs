using System;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class ProcessesWrapper
		: IProcesses
	{
		private readonly IProcesses _processes;
		private readonly string _machine;

		public ProcessesWrapper(IProcesses processes, string machine)
		{
			_processes = processes;
			_machine = machine;
		}

		#region Implementation of IProcesses

		public int KillAll(string name)
		{
			try
			{
				return _processes.KillAll(name);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		#endregion
	}
}