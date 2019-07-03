using System;
using System.Collections.Generic;
using System.Net;
using DPloy.Core.PublicApi;
using DPloy.Distributor.Output;

namespace DPloy.Distributor
{
	internal sealed class Distributor
		: IDisposable
	{
		private readonly List<RemoteNode> _clients;
		private readonly IOperationTracker _operationTracker;

		public Distributor(IOperationTracker operationTracker)
		{
			_operationTracker = operationTracker;
			_clients = new List<RemoteNode>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();
		}

		#endregion

		public RemoteNode ConnectTo(string address, TimeSpan connectTimeout)
		{
			return AddClient(() => RemoteNode.Create(_operationTracker, address, connectTimeout));
		}

		public INode ConnectTo(IPEndPoint ep)
		{
			return AddClient(() => RemoteNode.Create(_operationTracker, ep));
		}

		private RemoteNode AddClient(Func<RemoteNode> fn)
		{
			RemoteNode client = null;
			try
			{
				client = fn();
				_clients.Add(client);
				return client;
			}
			catch (Exception)
			{
				if (client != null)
				{
					client.Dispose();
					_clients.Remove(client);
				}
				throw;
			}
		}
	}
}