using System;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class ServicesWrapper
		: IServices
	{
		private readonly IServices _services;
		private readonly string _machine;

		public ServicesWrapper(IServices services, string machine)
		{
			_services = services;
			_machine = machine;
		}

		#region Implementation of IServices

		public void Start(string serviceName)
		{
			try
			{
				_services.Start(serviceName);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public void Stop(string serviceName)
		{
			try
			{
				_services.Stop(serviceName);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		#endregion
	}
}