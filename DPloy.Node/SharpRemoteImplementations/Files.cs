using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
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

		public Task DeleteAsync(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
				Log.InfoFormat("Delete file '{0}'", filePath);
			}

			return Task.FromResult(42);
		}

		public Task OpenFileAsync(string filePath, long fileSize)
		{
			Log.DebugFormat("Creating file '{0}'...", filePath);

			DeleteAsync(filePath);

			var directoryPath = Path.GetDirectoryName(filePath);
			CreateDirectoryIfNecessary(directoryPath);

			var fileStream = File.Create(filePath);
			_files.Add(filePath, fileStream);

			Log.InfoFormat("Created file '{0}'", filePath);

			return Task.FromResult(42);
		}

		public Task WriteAsync(string filePath, long position, byte[] buffer)
		{
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
			Log.DebugFormat("Closing '{0}'...", filePath);

			if (_files.TryGetValue(filePath, out var stream))
			{
				stream.Dispose();
			}

			Log.DebugFormat("Closed '{0}'...", filePath);

			return Task.FromResult(42);
		}

		public byte[] CalculateSha256(string filePath)
		{
			return HashCodeCalculator.Sha256(filePath);
		}

		public byte[] CalculateMD5(string filePath)
		{
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

		private void CopyFile(CopyFile file)
		{
			if (File.Exists(file.FileName))
				File.Delete(file.FileName);

			var directory = Path.GetDirectoryName(file.FileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			File.WriteAllBytes(file.FileName, file.Content);
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