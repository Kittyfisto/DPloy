using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DPloy.Core.Hash;
using DPloy.Core.PublicApi;
using DPloy.Core.SharpRemoteInterfaces;
using SharpRemote;

namespace DPloy.Core
{
	sealed class Client : IClient
	{
		private readonly SocketEndPoint _socket;
		private readonly IPaths _paths;
		private readonly IFile _file;
		private readonly IShell _shell;

		public Client(IPEndPoint endPoint)
		{
			_socket = new SocketEndPoint(EndPointType.Client);
			_socket.Connect(endPoint);

			_paths = _socket.GetExistingOrCreateNewProxy<IPaths>(ObjectIds.Paths);
			_file = _socket.GetExistingOrCreateNewProxy<IFile>(ObjectIds.File);
			_shell = _socket.GetExistingOrCreateNewProxy<IShell>(ObjectIds.Shell);
		}

		#region Implementation of IClient

		public void CopyFile(string sourceFilePath, string destinationPath)
		{
			using (var hash = new HashCodeCalculator())
			using (var sourceStream = File.OpenRead(sourceFilePath))
			{
				var fileSize = sourceStream.Length;
				var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourceFilePath));
				var lastTask = _file.CreateAsync(destinationFilePath, fileSize);

				const int bufferSize = 4096;
				var buffer = new byte[bufferSize];

				while (true)
				{
					var bytesRead = sourceStream.Read(buffer,
					                                  0,
					                                  buffer.Length);
					if (bytesRead <= 0)
						break;

					hash.Append(buffer, bytesRead);
					lastTask = AppendAsync(bytesRead, buffer, destinationFilePath);
				}

				lastTask.Wait();
				var thisHash = hash.CalculateHash();
				var clientHash = _file.CalculateHash(destinationFilePath);
				if (!AreEqual(thisHash, clientHash))
					throw new NotImplementedException("The hashes differ!");
			}
		}

		private Task AppendAsync(int bytesRead, byte[] buffer, string destinationFilePath)
		{
			Task lastTask;
			if (bytesRead < buffer.Length)
			{
				// Usually, the last block of a file transfer has a size of less than the buffer.
				// We could simply send the entire last block and include the 'bytesRead' as an additional parameter,
				// but this will waste a lot of network resources in case we're sending lots of small files.
				// Therefore we copy the buffer of the last block into a correctly sized temp buffer
				// and send that one instead.
				var finalBuffer = new byte[bytesRead];
				Array.Copy(buffer, finalBuffer, bytesRead);
				lastTask = _file.AppendAsync(destinationFilePath, finalBuffer);
			}
			else
			{
				lastTask = _file.AppendAsync(destinationFilePath, buffer);
			}

			return lastTask;
		}

		private static bool AreEqual(byte[] hash, byte[] otherHash)
		{
			if (hash.Length != otherHash.Length)
				return false;

			for (int i = 0; i < hash.Length; ++i)
			{
				if (hash[i] != otherHash[i])
					return false;
			}

			return true;
		}

		public void Install(string installerPath)
		{
			var destinationPath = Path.Combine(_paths.GetTempPath(), "DPloy", "Installers");
			var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(installerPath));

			CopyFile(installerPath, destinationPath);
			ExecuteFile(destinationFilePath);
		}

		public void ExecuteFile(string clientFilePath)
		{
			var returnCode = ExecuteCommand(clientFilePath);
		}

		public int ExecuteCommand(string cmd)
		{
			return _shell.Execute(cmd);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			_socket?.Dispose();
		}

		#endregion
	}
}