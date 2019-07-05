using System.Threading.Tasks;
using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	/// <summary>
	///     Provides remote access to the filesystem of a node.
	/// </summary>
	public interface IFiles
	{
		/// <summary>
		///     Verifies if there exists a file at the given location with the given file size and SHA256 hash.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="expectedFileSize"></param>
		/// <param name="expectedHash"></param>
		/// <returns></returns>
		bool Exists(string filePath, long expectedFileSize, byte[] expectedHash);

		/// <summary>
		///     Verifies if there exists a file at the given location.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		bool FileExists(string filePath);

		/// <summary>
		///     Deletes the given file.
		/// </summary>
		/// <remarks>
		///     If either the file OR the directory do NOT exist, then NOTHING happens.
		/// </remarks>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task DeleteFileAsync(string filePath);

		/// <summary>
		///     Deletes all files matching the given pattern.
		/// </summary>
		/// <remarks>
		///     If either the file OR the directory do NOT exist, then NOTHING happens.
		/// </remarks>
		/// <param name="filePathPattern"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task DeleteFilesAsync(string filePathPattern);

		/// <summary>
		///     Opens the given file for writing.
		///     If the file does not exist, it will be created.
		///     If the folder filePath does not exist, it will be created.
		///     The file should be closed using <see cref="CloseFileAsync" /> or it will stay open until
		///     the connection is dropped.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="fileSize"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task OpenFileAsync(string filePath, long fileSize);

		/// <summary>
		///     Writes the given block of data to the given file at the given position.
		///     The file needs to have been opened previously using <see cref="OpenFileAsync" />.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="position"></param>
		/// <param name="buffer"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task WriteAsync(string filePath, long position, byte[] buffer);

		/// <summary>
		///     Closes a file which has been previously created using <see cref="OpenFileAsync" />.
		///     Does nothing if the file is not open.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task CloseFileAsync(string filePath);

		/// <summary>
		///     Deletes a directory and all of its files.
		/// </summary>
		/// <param name="destinationDirectoryPath"></param>
		/// <param name="recursive">When set to true, sub-directories (and their files and subdirectories) are deleted</param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		Task DeleteDirectoryAsync(string destinationDirectoryPath, bool recursive);

		/// <summary>
		///     Creates a new directory.
		/// </summary>
		/// <param name="destinationDirectoryPath"></param>
		[Invoke(Dispatch.SerializePerObject)]
		Task CreateDirectoryAsync(string destinationDirectoryPath);

		/// <summary>
		///     Calculates and returns the hash of the given file using the SHA256 algorithm.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		byte[] CalculateSha256(string filePath);

		/// <summary>
		///     Calculates and returns the hash of the given file using the MD5 algorithm.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Invoke(Dispatch.SerializePerObject)]
		byte[] CalculateMD5(string filePath);

		/// <summary>
		///     Executes all instructions in the given batch.
		/// </summary>
		/// <param name="batch"></param>
		[Invoke(Dispatch.SerializePerObject)]
		Task ExecuteBatchAsync(FileBatch batch);

		/// <summary>
		///     Unzips the given zip archive into the given folder.
		/// </summary>
		/// <param name="archivePath"></param>
		/// <param name="destinationFolder"></param>
		/// <param name="overwrite"></param>
		[Invoke(Dispatch.SerializePerObject)]
		void Unzip(string archivePath, string destinationFolder, bool overwrite);
	}
}