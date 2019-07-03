using System;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class RegistryWrapper
		: IRegistry
	{
		private readonly IRegistry _registry;
		private readonly string _machine;

		public RegistryWrapper(IRegistry registry, string machine)
		{
			_registry = registry;
			_machine = machine;
		}

		public string GetStringValue(string keyName, string valueName)
		{
			try
			{
				return _registry.GetStringValue(keyName, valueName);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public uint GetDwordValue(string keyName, string valueName)
		{
			try
			{
				return _registry.GetDwordValue(keyName, valueName);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}
	}
}