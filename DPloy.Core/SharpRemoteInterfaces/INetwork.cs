using System.Threading.Tasks;

namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface INetwork
	{
		Task DownloadFileAsync(string sourceFileUri,
		                       string destinationFilePath);
	}
}
