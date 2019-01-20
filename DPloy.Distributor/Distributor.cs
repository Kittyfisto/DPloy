using System;
using System.Collections.Generic;
using System.Net;
using DPloy.Core.PublicApi;
using SharpRemote.ServiceDiscovery;

namespace DPloy.Distributor
{
	internal sealed class Distributor
		: IDistributor
		, IDisposable
	{
		private NetworkServiceDiscoverer _discoverer;
		private readonly List<NodeClient> _clients;
		private readonly ProgressWriter _progressWriter;

		public Distributor(ProgressWriter progressWriter)
		{
			_progressWriter = progressWriter;
			_clients = new List<NodeClient>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();
			_discoverer?.Dispose();
		}

		#endregion

		public INode ConnectTo(string addressOrDomain, int port)
		{
			var ep = new IPEndPoint(IPAddress.Parse(addressOrDomain), port);
			return AddClient(() => NodeClient.Create(_progressWriter, ep));
		}

		public INode ConnectTo(string computerName)
		{
			if (_discoverer == null)
				_discoverer = new NetworkServiceDiscoverer();
			return AddClient(() => NodeClient.Create(_progressWriter, _discoverer, computerName));
		}

		public INode ConnectTo(IPEndPoint ep)
		{
			return AddClient(() => NodeClient.Create(_progressWriter, ep));
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