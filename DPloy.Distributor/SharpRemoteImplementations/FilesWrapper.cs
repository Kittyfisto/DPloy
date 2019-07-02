using System;
using System.Threading.Tasks;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.SharpRemoteImplementations
{
	internal sealed class FilesWrapper
		: IFiles
	{
		private readonly IFiles _files;
		private readonly string _machine;

		public FilesWrapper(IFiles files, string machine)
		{
			_files = files;
			_machine = machine;
		}

		#region Implementation of IFiles

		public bool Exists(string filePath, long expectedFileSize, byte[] expectedHash)
		{
			try
			{
				return _files.Exists(filePath, expectedFileSize, expectedHash);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task DeleteFileAsync(string filePath)
		{
			try
			{
				await _files.DeleteFileAsync(filePath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task DeleteFilesAsync(string filePathPattern)
		{
			try
			{
				await _files.DeleteFilesAsync(filePathPattern);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task OpenFileAsync(string filePath, long fileSize)
		{
			try
			{
				await _files.OpenFileAsync(filePath, fileSize);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task WriteAsync(string filePath, long position, byte[] buffer)
		{
			try
			{
				await _files.WriteAsync(filePath, position, buffer);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task CloseFileAsync(string filePath)
		{
			try
			{
				await _files.CloseFileAsync(filePath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task DeleteDirectoryAsync(string destinationDirectoryPath, bool recursive)
		{
			try
			{
				await _files.DeleteDirectoryAsync(destinationDirectoryPath, recursive);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task CreateDirectoryAsync(string destinationDirectoryPath)
		{
			try
			{
				await _files.CreateDirectoryAsync(destinationDirectoryPath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public byte[] CalculateSha256(string filePath)
		{
			try
			{
				return _files.CalculateSha256(filePath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public byte[] CalculateMD5(string filePath)
		{
			try
			{
				return _files.CalculateMD5(filePath);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public async Task ExecuteBatchAsync(FileBatch batch)
		{
			try
			{
				await _files.ExecuteBatchAsync(batch);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		public void Unzip(string archivePath, string destinationFolder, bool overwrite)
		{
			try
			{
				_files.Unzip(archivePath, destinationFolder, overwrite);
			}
			catch (Exception e)
			{
				throw new RemoteNodeException(_machine, e);
			}
		}

		#endregion
	}
}
