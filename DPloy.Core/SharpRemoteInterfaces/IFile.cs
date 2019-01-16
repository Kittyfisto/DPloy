using System.Threading.Tasks;
using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface IFile
	{
		/// <summary>
		/// Deletes the given file.
		/// </summary>
		/// <remarks>
		/// If either the file OR the directory do NOT exist, then NOTHING happens.
		/// </remarks>
		/// <param name="path"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task DeleteAsync(string path);

		[Invoke(Dispatch.SerializePerObject)]
		Task CreateAsync(string destinationPath, long fileSize);

		[Invoke(Dispatch.SerializePerObject)]
		Task AppendAsync(string destinationPath, byte[] buffer);

		[Invoke(Dispatch.SerializePerObject)]
		byte[] CalculateHash(string destinationPath);
	}
}