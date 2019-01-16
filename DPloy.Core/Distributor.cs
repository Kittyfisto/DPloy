using System;
using System.Collections.Generic;
using System.Net;
using DPloy.Core.PublicApi;

namespace DPloy.Core
{
	public sealed class Distributor
		: IDistributor
		, IDisposable
	{
		private readonly List<Client> _clients;

		public Distributor()
		{
			_clients = new List<Client>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();
		}

		#endregion

		public IClient ConnectTo(string addressOrDomain, int port)
		{
			var ep = new IPEndPoint(IPAddress.Parse(addressOrDomain), port);
			return ConnectTo(ep);
		}

		public IClient ConnectTo(IPEndPoint ep)
		{
			var client = new Client(ep);
			_clients.Add(client);
			return client;
		}
	}
}