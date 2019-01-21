using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DPloy.Core;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Node.SharpRemoteImplementations
{
	internal sealed class Network
		: INetwork
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of INetwork

		public Task DownloadFileAsync(string sourceFileUri, string destinationFilePath)
		{
			Log.InfoFormat("Downloading '{0}' to '{1}'...", sourceFileUri, destinationFilePath);

			var finalDestinationPath = Paths.NormalizeAndEvaluate(destinationFilePath);
			var finalDestinationDir = Path.GetDirectoryName(finalDestinationPath);
			Directory.CreateDirectory(finalDestinationDir);

			using (var client = new WebClient())
			{
				client.DownloadFile(sourceFileUri, finalDestinationPath);
			}
			
			Log.InfoFormat("Downloading '{0}' to '{1}' finished", sourceFileUri, destinationFilePath);

			return Task.FromResult(42);
		}

		#endregion
	}
}
