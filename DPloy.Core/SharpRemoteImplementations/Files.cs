using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DPloy.Core.Hash;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Core.SharpRemoteImplementations
{
	public sealed class Files
		: IFiles
	{
		private readonly IFilesystem _filesystem;
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Dictionary<string, Stream> _files;

		public Files(IFilesystem filesystem)
		{
			_filesystem = filesystem;
			_files = new Dictionary<string, Stream>();
		}

		#region Implementation of IFiles

		public bool Exists(string filePath, long expectedFileSize, byte[] expectedHash)
		{
			filePath = NormalizeAndEvaluate(filePath);

			var info = _filesystem.GetFileInfo(filePath);

			if (!info.Exists)
			{
				Log.DebugFormat("The file '{0}' does not exist", filePath);
				return false;
			}

			if (info.Length != expectedFileSize)
			{
				Log.DebugFormat("The file '{0}' exists, but has a different '{1}' filesize than the expected '{2}'",
					filePath, info.Length, expectedFileSize);
				return false;
			}

			var hash = HashCodeCalculator.MD5(filePath);
			if (!HashCodeCalculator.AreEqual(hash, expectedHash))
			{
				Log.DebugFormat(
					"The file '{0}' exists, has the expected size '{1}', but it's hash '{2}' differs from the expected one '{3}'",
					filePath,
					info.Length,
					HashCodeCalculator.ToString(hash),
					HashCodeCalculator.ToString(expectedHash));
				return false;
			}

			Log.DebugFormat(
				"The file '{0}' exists, has the expected size '{1}' and hash '{2}'",
				filePath,
				info.Length,
				HashCodeCalculator.ToString(hash));
			return true;
		}

		public bool FileExists(string filePath)
		{
			var absoluteFilePath = NormalizeAndEvaluate(filePath);
			return _filesystem.FileExists(absoluteFilePath);
		}

		public Task DeleteFileAsync(string filePath)
		{
			DeleteFile(filePath);
			return Task.FromResult(42);
		}

		public Task DeleteFilesAsync(string filePathPattern)
		{
			var path = Path.GetDirectoryName(filePathPattern);
			var fileName = Path.GetFileName(filePathPattern);

			var absolutePath = NormalizeAndEvaluate(path);
			var files = _filesystem.EnumerateFiles(absolutePath, fileName, SearchOption.TopDirectoryOnly, tolerateNonExistantPath: true);

			foreach(var file in files)
				DeleteFile(file);

			return Task.FromResult(42);
		}

		public Task OpenFileAsync(string filePath, long fileSize)
		{
			filePath = NormalizeAndEvaluate(filePath);

			Log.DebugFormat("Creating file '{0}'...", filePath);

			DeleteFile(filePath);

			var directoryPath = Path.GetDirectoryName(filePath);
			CreateDirectoryIfNecessary(directoryPath);

			var fileStream = _filesystem.OpenWrite(filePath);
			_files.Add(filePath, fileStream);

			Log.InfoFormat("Created file '{0}'", filePath);

			return Task.FromResult(42);
		}

		public Task WriteAsync(string filePath, long position, byte[] buffer)
		{
			filePath = NormalizeAndEvaluate(filePath);

			Log.DebugFormat("Writing to '{0}': @{1}, {2} bytes...", filePath, position, buffer.Length);

			if (!_files.TryGetValue(filePath, out var stream))
				throw new InvalidOperationException($"No file named '{filePath}' has been opened");

			stream.Position = position;
			stream.Write(buffer, 0, buffer.Length);

			Log.DebugFormat("Finished writing to '{0}': @{1}, {2} bytes", filePath, position, buffer.Length);

			return Task.FromResult(42);
		}

		public Task CloseFileAsync(string filePath)
		{
			filePath = NormalizeAndEvaluate(filePath);

			Log.DebugFormat("Closing '{0}'...", filePath);

			if (_files.TryGetValue(filePath, out var stream))
			{
				stream.Dispose();
			}

			Log.DebugFormat("Closed '{0}'", filePath);

			return Task.FromResult(42);
		}

		public Task DeleteDirectoryAsync(string destinationDirectoryPath, bool recursive)
		{
			var normalizedPath = NormalizeAndEvaluate(destinationDirectoryPath);

			Log.DebugFormat("Deleting '{0}'...", normalizedPath);

			DeleteDirectoryPrivate(normalizedPath, recursive);

			Log.DebugFormat("Deleted '{0}'", normalizedPath);

			return Task.FromResult(42);
		}

		public Task CreateDirectoryAsync(string destinationDirectoryPath)
		{
			var normalizedPath = NormalizeAndEvaluate(destinationDirectoryPath);

			Log.DebugFormat("Creating '{0}'...", normalizedPath);

			Directory.CreateDirectory(normalizedPath);

			Log.DebugFormat("Created '{0}'", normalizedPath);

			return Task.FromResult(42);
		}

		private static void DeleteDirectoryPrivate(string normalizedPath, bool recursive)
		{
			const int maxTries = 3;

			Exception exception = null;
			for (int i = 0; i < maxTries; ++i)
			{
				if (TryDeleteDirectory(normalizedPath, recursive, out exception))
					return;

				// With every failure we wait a little bit longer...
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * (i+1)));
			}

			throw exception ?? new NotImplementedException();
		}

		private static bool TryDeleteDirectory(string normalizedPath, bool recursive, out Exception exception)
		{
			try
			{
				Directory.Delete(normalizedPath, recursive);

				exception = null;
				return true;
			}
			catch (DirectoryNotFoundException e)
			{
				Log.DebugFormat("Ignoring exception:\r\n{0}", e);

				exception = null;
				return true;
			}
			catch (UnauthorizedAccessException e)
			{
				Log.DebugFormat("Caught exception while trying to delete folder '{0}':\r\n{1}", normalizedPath, e);

				RepairAccess(normalizedPath);
				exception = null;
				return false;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		private static void RepairAccess(string normalizedPath)
		{
			Log.DebugFormat("Trying to repair access to '{0}'...", normalizedPath);

			foreach (var file in Directory.EnumerateFiles(normalizedPath, "*", SearchOption.AllDirectories))
			{
				try
				{
					File.Delete(file);
				}
				catch (UnauthorizedAccessException e)
				{
					Log.DebugFormat("Unable to access '{0}', changing file attributes to normal...", e);
					File.SetAttributes(file, FileAttributes.Normal);
				}
			}
		}

		public byte[] CalculateSha256(string filePath)
		{
			filePath = NormalizeAndEvaluate(filePath);

			using (var algorithm = SHA256.Create())
			using (var stream = _filesystem.OpenRead(filePath))
			{
				return HashCodeCalculator.CalculateHash(stream, algorithm);
			}
		}

		public byte[] CalculateMD5(string filePath)
		{
			filePath = NormalizeAndEvaluate(filePath);

			using (var algorithm = MD5.Create())
			using (var stream = _filesystem.OpenRead(filePath))
			{
				return HashCodeCalculator.CalculateHash(stream, algorithm);
			}
		}

		public Task ExecuteBatchAsync(FileBatch batch)
		{
			foreach (var file in batch.FilesToCreate)
			{
				CopyFile(file);
			}

			return Task.FromResult(42);
		}

		public void Unzip(string archivePath, string destinationFolder, bool overwrite)
		{
			var actualArchivePath = NormalizeAndEvaluate(archivePath);
			var actualDestinationFolder = NormalizeAndEvaluate(destinationFolder);

			using (var stream = _filesystem.OpenRead(actualArchivePath))
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, true))
			{
				foreach (var entry in archive.Entries)
				{
					using (var source = entry.Open())
					{
						var destinationFilePath = Path.Combine(actualDestinationFolder, entry.FullName);

						if (overwrite)
							DeleteFile(destinationFilePath);
						else if (_filesystem.FileExists(destinationFilePath))
							throw new IOException($"The file '{destinationFilePath}' already exists");

						var destinationPath = Path.GetDirectoryName(destinationFilePath);
						CreateDirectoryIfNecessary(destinationPath);

						using (var destination = _filesystem.OpenWrite(destinationFilePath))
						{
							source.CopyTo(destination);
						}
					}
				}
			}
		}

		private void DeleteFile(string filePath)
		{
			filePath = NormalizeAndEvaluate(filePath);

			if (_filesystem.FileExists(filePath))
			{
				Log.DebugFormat("Deleting file '{0}'...", filePath);
				_filesystem.DeleteFile(filePath);
				Log.InfoFormat("Deleted file '{0}'", filePath);
			}
		}

		private void CopyFile(CreateFile file)
		{
			var filePath = NormalizeAndEvaluate(file.FilePath);

			DeleteFile(filePath);
			var directory = Path.GetDirectoryName(filePath);
			_filesystem.CreateDirectory(directory);
			_filesystem.WriteAllBytes(filePath, file.Content);
		}

		#endregion

		private void CreateDirectoryIfNecessary(string directoryPath)
		{
			Log.DebugFormat("Creating directory '{0}'...", directoryPath);

			if (!_filesystem.DirectoryExists(directoryPath))
			{
				_filesystem.CreateDirectory(directoryPath);
				Log.InfoFormat("Created directory '{0}'", directoryPath);
			}
		}

		public void CloseAll()
		{
			foreach (var file in _files)
			{
				try
				{
					file.Value?.Close();
				}
				catch (Exception e)
				{
					Log.WarnFormat("Unable to close file '{0}':\r\n{1}",
						file.Key, e);
				}
			}
			_files.Clear();
		}

		private string NormalizeAndEvaluate(string relativeOrAbsolutePath)
		{
			return Paths.NormalizeAndEvaluate(relativeOrAbsolutePath, _filesystem.CurrentDirectory);
		}
	}
}