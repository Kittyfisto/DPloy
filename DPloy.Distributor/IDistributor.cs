using DPloy.Core.PublicApi;

namespace DPloy.Distributor
{
	public interface IDistributor
	{
		/// <summary>
		///     Establishes a connection with the given client and returns an API
		///     which allows file-system manipulation as well as command execution.
		/// </summary>
		/// <remarks>
		///     The returned object should be disposed of when it is no longer needed.
		/// </remarks>
		/// <param name="addressOrDomain"></param>
		/// <returns></returns>
		INode ConnectTo(string addressOrDomain);
	}
}