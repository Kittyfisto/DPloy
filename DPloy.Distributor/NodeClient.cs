using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DPloy.Core;
using DPloy.Core.Hash;
using DPloy.Core.PublicApi;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;
using SharpRemote;
using SharpRemote.ServiceDiscovery;

namespace DPloy.Distributor
{
	/// <summary>
	///     Implementation of the public node interface - responsible for performing
	///     the actual remote procedure calls to the remote computer.
	/// </summary>
	/// <remarks>
	///     This is the counterpart of the 'NodeServer' class in the DPloy.Node project.
	/// </remarks>
	internal sealed class NodeClient : INode
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IFiles _files;
		private readonly IShell _shell;
		private readonly IServices _services;
		private readonly SocketEndPoint _socket;

		private const int FilePacketBufferSize = 1024 * 1024 * 4;

		private NodeClient(SocketEndPoint socket)
		{
			_socket = socket;
			_files = _socket.GetExistingOrCreateNewProxy<IFiles>(ObjectIds.File);
			_shell = _socket.GetExistingOrCreateNewProxy<IShell>(ObjectIds.Shell);
			_services = _socket.GetExistingOrCreateNewProxy<IServices>(ObjectIds.Services);
		}

		#region IDisposable

		public void Dispose()
		{
			_socket?.Dispose();
		}

		#endregion

		public static NodeClient Create(IPEndPoint endPoint)
		{
			var socket = new SocketEndPoint(EndPointType.Client, "Distributor",
				clientAuthenticator: MachineNameAuthenticator.CreateClient(),
				heartbeatSettings: new HeartbeatSettings
				{
					AllowRemoteHeartbeatDisable = true
				});
			try
			{
				socket.Connect(endPoint);
				return new NodeClient(socket);
			}
			catch (Exception)
			{
				socket.Dispose();
				throw;
			}
		}

		public static NodeClient Create(INetworkServiceDiscoverer discoverer, string computerName)
		{
			var socket = new SocketEndPoint(EndPointType.Client, "Distributor",
				clientAuthenticator: MachineNameAuthenticator.CreateClient(),
				networkServiceDiscoverer: discoverer,
				heartbeatSettings: new HeartbeatSettings
				{
					AllowRemoteHeartbeatDisable = true
				});
			try
			{
				var serviceName = $"{computerName}.DPloy.Node";

				var services = discoverer.FindServices(serviceName);

				socket.Connect(serviceName);
				return new NodeClient(socket);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		#region Implementation of IClient

		public void CopyFile(string sourceFilePath, string destinationFolder)
		{
			CopyFilePrivate(Paths.NormalizeAndEvaluate(sourceFilePath), destinationFolder);
		}

		private void CopyFilePrivate(string sourceFilePath, string destinationFolder)
		{
			var fileSize = new FileInfo(sourceFilePath).Length;
			if (IsSmallFile(fileSize))
			{
				CopyFileBatch(destinationFolder, new[]{sourceFilePath});
			}
			else
			{
				CopyFileChunked(sourceFilePath, destinationFolder, fileSize);
			}
		}

		public void CopyFiles(IEnumerable<string> sourceFiles, string destinationFolder)
		{
			CopyFilesPrivate(sourceFiles.Select(Paths.NormalizeAndEvaluate).ToArray(), destinationFolder);
		}

		public void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder)
		{
			var destinationArchiveFolder = @"%temp%";
			CopyFile(sourceArchivePath, destinationArchiveFolder);

			UnzipArchive(sourceArchivePath, destinationFolder, destinationArchiveFolder, overwrite: true);
		}

		private void UnzipArchive(string sourceArchivePath, string destinationFolder, string tmpFolder, bool overwrite)
		{
			Log.InfoFormat("Unzipping '{0}' into '{1}'", sourceArchivePath, destinationFolder);

			var destinationArchivePath = Path.Combine(tmpFolder, Path.GetFileName(sourceArchivePath));
			_files.Unzip(destinationArchivePath, destinationFolder, overwrite);
		}

		public void Install(string installerPath, string commandLine = null)
		{
			InstallPrivate(Paths.NormalizeAndEvaluate(installerPath), commandLine);
		}

		public void ExecuteFile(string clientFilePath, string commandLine = null)
		{
			var command = $"\"{clientFilePath}\" {commandLine}";
			var returnCode = ExecuteCommand(command);
			if (returnCode != 0)
				throw new Exception($"The command {command} returned {returnCode}");
		}

		public int ExecuteCommand(string cmd)
		{
			Log.InfoFormat("Executing command '{0}'...", cmd);

			return _shell.Execute(cmd);
		}

		public void StartService(string serviceName)
		{
			Log.InfoFormat("Starting service '{0}'...", serviceName);

			_services.Start(serviceName);
		}

		public void StopService(string serviceName)
		{
			Log.InfoFormat("Stopping service '{0}'...", serviceName);

			_services.Stop(serviceName);
		}

		#endregion

		private void InstallPrivate(string installerPath, string commandLine)
		{
			var destinationPath = Path.Combine(Paths.Temp, "DPloy", "Installers");
			var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(installerPath));

			CopyFile(installerPath, destinationPath);
			ExecuteFile(destinationFilePath, commandLine ?? "/S");
		}

		private void CopyFilesPrivate(IEnumerable<string> sourceFiles, string destinationFolder)
		{
			var smallFiles = new List<string>();
			var bigFiles = new List<string>();
			ClassifyFilesBySize(sourceFiles, smallFiles, bigFiles);

			foreach (var bigFile in bigFiles)
			{
				CopyFile(bigFile, destinationFolder);
			}

			CopyFileBatch(destinationFolder, smallFiles);
		}

		private bool Exists(string destinationFilePath, long expectedFileSize, byte[] expectedHash)
		{
			return _files.Exists(destinationFilePath, expectedFileSize, expectedHash);
		}

		private Task WriteAsync(int bytesRead, long position, byte[] buffer, string destinationFilePath)
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
				lastTask = _files.WriteAsync(destinationFilePath, position, finalBuffer);
			}
			else
			{
				lastTask = _files.WriteAsync(destinationFilePath, position, buffer);
			}

			return lastTask;
		}

		private static void ClassifyFilesBySize(IEnumerable<string> sourceFiles, List<string> smallFiles, List<string> bigFiles)
		{
			foreach (var fileName in sourceFiles)
			{
				if (IsSmallFile(new FileInfo(fileName).Length))
				{
					smallFiles.Add(fileName);
				}
				else
				{
					bigFiles.Add(fileName);
				}
			}
		}

		[Pure]
		private static bool IsSmallFile(long fileSize)
		{
			return fileSize <= FilePacketBufferSize / 2;
		}

		/// <summary>
		///    Copies one file in one or more chunks, if need be.
		///    Currently a chunk can be up to 4MB in size.
		/// </summary>
		/// <remarks>
		///    Before the file is copied, a check is performed if sending the file
		///    is necessary. If the file already exists on the target system (and the content
		///    is identical), then nothing more is done.
		/// </remarks>
		/// <param name="sourceFilePath"></param>
		/// <param name="destinationFolder"></param>
		/// <param name="expectedFileSize"></param>
		private void CopyFileChunked(string sourceFilePath, string destinationFolder, long expectedFileSize)
		{
			var destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
			var thisHash = HashCodeCalculator.MD5(sourceFilePath);
			if (Exists(destinationFilePath, expectedFileSize, thisHash))
			{
				Log.InfoFormat("Skipping copy of '{0}' to '{1}' because the target file is already present", sourceFilePath,
				               destinationFolder);
				return;
			}

			Log.InfoFormat("Copying '{0}' to '{1}'...", sourceFilePath, destinationFolder);

			using (var sourceStream = File.OpenRead(sourceFilePath))
			{
				var fileSize = sourceStream.Length;
				var tasks = new List<Task>
				{
					_files.OpenFileAsync(destinationFilePath, fileSize)
				};


				var buffer = new byte[FilePacketBufferSize];

				while (true)
				{
					var position = sourceStream.Position;
					var bytesRead = sourceStream.Read(buffer, 0, buffer.Length);
					if (bytesRead <= 0)
						break;

					tasks.Add(WriteAsync(bytesRead, position, buffer, destinationFilePath));
				}

				tasks.Add(_files.CloseFileAsync(destinationFilePath));

				Task.WaitAll(tasks.ToArray());
				var clientHash = _files.CalculateMD5(destinationFilePath);
				if (!HashCodeCalculator.AreEqual(thisHash, clientHash))
					throw new
						NotImplementedException($"There has been an error while copying '{sourceFilePath}' to '{destinationFilePath}', the resulting hashes should be identical, but are not!");
			}
		}

		private void CopyFileBatch(string destinationFolder, IReadOnlyList<string> smallFiles)
		{
			var tasks = new List<Task>();

			var batch = new FileBatch();
			long length = 0;
			foreach (var fileName in smallFiles)
			{
				var instruction = new CopyFile
				{
					FilePath = Path.Combine(destinationFolder, Path.GetFileName(fileName)),
					Content = File.ReadAllBytes(fileName)
				};
				batch.FilesToCopy.Add(instruction);
				length += instruction.Content.Length;

				if (length >= FilePacketBufferSize)
				{
					tasks.Add(_files.ExecuteBatchAsync(batch));
					batch = new FileBatch();
					length = 0;
				}
			}

			if (batch.Any())
			{
				tasks.Add(_files.ExecuteBatchAsync(batch));
			}

			Task.WaitAll(tasks.ToArray());
		}
	}
}