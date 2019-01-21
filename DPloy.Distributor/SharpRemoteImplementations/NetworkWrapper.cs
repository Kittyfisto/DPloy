using System;
using System.Threading.Tasks;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class NetworkWrapper
		: INetwork
	{
		private readonly INetwork _network;
		private readonly string _remoteMachineName;

		public NetworkWrapper(INetwork network, string remoteMachineName)
		{
			_network = network;
			_remoteMachineName = remoteMachineName;
		}

		#region Implementation of INetwork

		public async Task DownloadFileAsync(string sourceFileUri, string destinationFilePath)
		{
			try
			{
				await _network.DownloadFileAsync(sourceFileUri, destinationFilePath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_remoteMachineName, e);
			}
		}

		#endregion
	}
}