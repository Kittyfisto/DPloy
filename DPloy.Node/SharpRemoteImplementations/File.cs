using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DPloy.Core.Hash;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Node.SharpRemoteImplementations
{
	sealed class File
		: IFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Implementation of IFile

		public Task DeleteAsync(string path)
		{
			if (System.IO.File.Exists(path))
				System.IO.File.Delete(path);

			return Task.FromResult(42);
		}

		public Task CreateAsync(string destinationPath, long fileSize)
		{
			DeleteAsync(destinationPath);

			var directoryPath = Path.GetDirectoryName(destinationPath);
			CreateDirectoryIfNecessary(directoryPath);

			using (System.IO.File.Create(destinationPath))
			{}
			return Task.FromResult(42);
		}

		private static void CreateDirectoryIfNecessary(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				Log.InfoFormat("Created directory '{0}'", directoryPath);
			}
		}

		public Task AppendAsync(string destinationPath, byte[] buffer)
		{
			using (var stream = System.IO.File.OpenWrite(destinationPath))
			{
				stream.Position = stream.Length;
				stream.Write(buffer, 0, buffer.Length);
			}

			return Task.FromResult(42);
		}

		public byte[] CalculateHash(string destinationPath)
		{
			using (var calculator = new HashCodeCalculator())
			using (var stream = System.IO.File.OpenRead(destinationPath))
			{
				const int bufferSize = 4096;
				var buffer = new Byte[bufferSize];

				while (true)
				{
					int bytesRead = stream.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0)
						break;

					calculator.Append(buffer, bytesRead);
				}

				return calculator.CalculateHash();
			}
		}

		#endregion
	}
}