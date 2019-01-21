using System;
using System.Collections.Generic;
using System.Net;
using DPloy.Core.PublicApi;

namespace DPloy.Distributor
{
	internal sealed class Distributor
		: IDistributor
		, IDisposable
	{
		private readonly List<NodeClient> _clients;
		private readonly ConsoleWriter _consoleWriter;

		public Distributor(ConsoleWriter consoleWriter)
		{
			_consoleWriter = consoleWriter;
			_clients = new List<NodeClient>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();
		}

		#endregion

		public INode ConnectTo(string addressOrDomain, int port)
		{
			var ep = new IPEndPoint(IPAddress.Parse(addressOrDomain), port);
			return AddClient(() => NodeClient.Create(_consoleWriter, ep));
		}

		public INode ConnectTo(string computerName)
		{
			return AddClient(() => NodeClient.Create(_consoleWriter, computerName));
		}

		public INode ConnectTo(IPEndPoint ep)
		{
			return AddClient(() => NodeClient.Create(_consoleWriter, ep));
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