using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DPloy.Core;
using DPloy.Core.Hash;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Node.SharpRemoteImplementations
{
	sealed class Files
		: IFiles
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly Dictionary<string, FileStream> _files;

		public Files()
		{
			_files = new Dictionary<string, FileStream>();
		}

		#region Implementation of IFiles

		public bool Exists(string filePath, long expectedFileSize, byte[] expectedHash)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

			if (!File.Exists(filePath))
			{
				Log.DebugFormat("The file '{0}' does not exist", filePath);
				return false;
			}

			var info = new FileInfo(filePath);
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

		public Task DeleteFileAsync(string filePath)
		{
			DeleteFile(filePath);
			return Task.FromResult(42);
		}

		public Task OpenFileAsync(string filePath, long fileSize)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

			Log.DebugFormat("Creating file '{0}'...", filePath);

			DeleteFile(filePath);

			var directoryPath = Path.GetDirectoryName(filePath);
			CreateDirectoryIfNecessary(directoryPath);

			var fileStream = File.Create(filePath);
			_files.Add(filePath, fileStream);

			Log.InfoFormat("Created file '{0}'", filePath);

			return Task.FromResult(42);
		}

		public Task WriteAsync(string filePath, long position, byte[] buffer)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

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
			filePath = Paths.NormalizeAndEvaluate(filePath);

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
			var normalizedPath = Paths.NormalizeAndEvaluate(destinationDirectoryPath);

			Log.DebugFormat("Deleting '{0}'...", normalizedPath);

			DeleteDirectoryPrivate(normalizedPath, recursive);

			Log.DebugFormat("Deleted '{0}'", normalizedPath);

			return Task.FromResult(42);
		}

		public Task CreateDirectoryAsync(string destinationDirectoryPath)
		{
			var normalizedPath = Paths.NormalizeAndEvaluate(destinationDirectoryPath);

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
				if (CanDeleteDirectory(normalizedPath, recursive, out exception))
					return;

				// With every failure we wait a little bit longer...
				Thread.Sleep(TimeSpan.FromMilliseconds(100 * (i+1)));
			}

			throw exception ?? new NotImplementedException();
		}

		private static bool CanDeleteDirectory(string normalizedPath, bool recursive, out Exception exception)
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
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		public byte[] CalculateSha256(string filePath)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

			return HashCodeCalculator.Sha256(filePath);
		}

		public byte[] CalculateMD5(string filePath)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

			return HashCodeCalculator.MD5(filePath);
		}

		public Task ExecuteBatchAsync(FileBatch batch)
		{
			foreach (var file in batch.FilesToCopy)
			{
				CopyFile(file);
			}

			return Task.FromResult(42);
		}

		public void Unzip(string archivePath, string destinationFolder, bool overwrite)
		{
			var actualArchivePath = Paths.NormalizeAndEvaluate(archivePath);
			var actualDestinationFolder = Paths.NormalizeAndEvaluate(destinationFolder);

			using (var archive = ZipFile.OpenRead(actualArchivePath))
			{
				foreach (var entry in archive.Entries)
				{
					using (var source = entry.Open())
					{
						var destinationFilePath = Path.Combine(actualDestinationFolder, entry.FullName);

						if (overwrite)
							DeleteFile(destinationFilePath);
						else if (File.Exists(destinationFilePath))
							throw new IOException($"The file '{destinationFilePath}' already exists");

						var destinationPath = Path.GetDirectoryName(destinationFilePath);
						CreateDirectoryIfNecessary(destinationPath);

						using (var destination = File.OpenWrite(destinationFilePath))
						{
							source.CopyTo(destination);
						}
					}
				}
			}
		}

		private static void DeleteFile(string filePath)
		{
			filePath = Paths.NormalizeAndEvaluate(filePath);

			if (File.Exists(filePath))
			{
				Log.DebugFormat("Deleting file '{0}'...", filePath);
				File.Delete(filePath);
				Log.InfoFormat("Deleted file '{0}'", filePath);
			}
		}

		private void CopyFile(CopyFile file)
		{
			var filePath = Paths.NormalizeAndEvaluate(file.FilePath);

			if (File.Exists(filePath))
				File.Delete(filePath);

			var directory = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllBytes(filePath, file.Content);
		}

		#endregion

		private static void CreateDirectoryIfNecessary(string directoryPath)
		{
			Log.DebugFormat("Creating directory '{0}'...", directoryPath);

			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
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
	}
}