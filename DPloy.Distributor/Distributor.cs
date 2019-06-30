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
		private readonly List<NodeClient> _clients;
		private readonly IOperationTracker _operationTracker;

		public Distributor(IOperationTracker operationTracker)
		{
			_operationTracker = operationTracker;
			_clients = new List<NodeClient>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();
		}

		#endregion

		public INode ConnectTo(string address, TimeSpan connectTimeout)
		{
			return AddClient(() => NodeClient.Create(_operationTracker, address, connectTimeout));
		}

		public INode ConnectTo(IPEndPoint ep)
		{
			return AddClient(() => NodeClient.Create(_operationTracker, ep));
		}

		private INode AddClient(Func<NodeClient> fn)
		{
			NodeClient client = null;
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