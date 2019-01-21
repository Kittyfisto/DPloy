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
using DPloy.Distributor.SharpRemoteImplementations;
using log4net;
using SharpRemote;

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

		private readonly ConsoleWriter _consoleWriter;
		private readonly IFiles _files;
		private readonly IShell _shell;
		private readonly IServices _services;
		private readonly IProcesses _processes;
		private readonly SocketEndPoint _socket;

		private const int FilePacketBufferSize = 1024 * 1024 * 4;

		private NodeClient(ConsoleWriter consoleWriter, SocketEndPoint socket, string remoteMachineName)
		{
			_consoleWriter = consoleWriter;
			_socket = socket;

			_files = new FilesWrapper(_socket.GetExistingOrCreateNewProxy<IFiles>(ObjectIds.File),
			                          remoteMachineName);
			_shell = new ShellWrapper(_socket.GetExistingOrCreateNewProxy<IShell>(ObjectIds.Shell),
			                          remoteMachineName);
			_services = new ServicesWrapper(_socket.GetExistingOrCreateNewProxy<IServices>(ObjectIds.Services),
			                                remoteMachineName);
			_processes = new ProcessesWrapper(_socket.GetExistingOrCreateNewProxy<IProcesses>(ObjectIds.Processes),
			                                  remoteMachineName);
		}

		#region IDisposable

		public void Dispose()
		{
			var socket = _socket;
			if (socket != null)
			{
				var operation = _consoleWriter.BeginDisconnect(socket.RemoteEndPoint);
				try
				{
					socket.Disconnect();
					operation.Success();
				}
				catch (Exception e)
				{
					operation.Failed(e);
				}
				finally
				{
					_socket?.Dispose();
				}
			}
		}

		#endregion

		public static NodeClient Create(ConsoleWriter consoleWriter, IPEndPoint endPoint)
		{
			var operation = consoleWriter.BeginConnect(endPoint.ToString());

			try
			{
				var node = CreatePrivate(consoleWriter, endPoint);
				operation.Success();
				return node;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public static NodeClient Create(ConsoleWriter consoleWriter, string computerName)
		{
			var operation = consoleWriter.BeginConnect(computerName);

			try
			{
				var node = CreatePrivate(consoleWriter, computerName);
				operation.Success();
				return node;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		private static NodeClient CreatePrivate(ConsoleWriter consoleWriter, string computerName)
		{
			var addresses = Dns.GetHostAddresses(computerName);
			if (addresses == null || addresses.Length == 0)
				throw new ArgumentException($"Unable to resolve '{computerName}' to an IPAddress - is this machine reachable?");

			var address = addresses[0];
			var socket = new SocketEndPoint(EndPointType.Client, "Distributor",
				clientAuthenticator: MachineNameAuthenticator.CreateClient(),
				heartbeatSettings: new HeartbeatSettings
				{
					AllowRemoteHeartbeatDisable = true
				});
			try
			{
				socket.Connect(new IPEndPoint(address, Constants.ConnectionPort));
				return new NodeClient(consoleWriter, socket, computerName);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		private static NodeClient CreatePrivate(ConsoleWriter consoleWriter, IPEndPoint endPoint)
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
				return new NodeClient(consoleWriter, socket, endPoint.ToString());
			}
			catch (Exception)
			{
				socket.Dispose();
				throw;
			}
		}

		#region Implementation of IClient

		public void KillProcesses(string processName)
		{
			var operation = _consoleWriter.BeginKillProcesses(processName);
			try
			{
				KillProcessesPrivate(processName);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CreateFile(string destinationFilePath, byte[] content)
		{
			var operation = _consoleWriter.BeginCreateFile(destinationFilePath);
			try
			{
				CreateFilePrivate(destinationFilePath, content);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CopyFile(string sourceFilePath, string destinationFilePath)
		{
			var operation = _consoleWriter.BeginCopyFile(sourceFilePath, destinationFilePath);
			try
			{
				CopyFilePrivate(Paths.NormalizeAndEvaluate(sourceFilePath), destinationFilePath);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder)
		{
			var operation = _consoleWriter.BeginCopyFiles(sourceFilePaths.ToList(), destinationFolder);
			try
			{
				CopyFilesPrivate(sourceFilePaths.Select(Paths.NormalizeAndEvaluate).ToArray(), destinationFolder);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CreateDirectory(string destinationDirectoryPath)
		{
			var operation = _consoleWriter.BeginCreateDirectory(destinationDirectoryPath);
			try
			{
				CreateDirectoryPrivate(destinationDirectoryPath);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var operation = _consoleWriter.BeginCopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
			try
			{
				var sourceFiles = Directory.EnumerateFiles(Paths.NormalizeAndEvaluate(sourceDirectoryPath)).ToList();
				if (sourceFiles.Any())
				{
					CopyFilesPrivate(sourceFiles, destinationDirectoryPath);
				}
				else
				{
					CreateDirectoryPrivate(destinationDirectoryPath);
				}

				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var operation = _consoleWriter.BeginCopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
			try
			{
				CopyDirectoryRecursivePrivate(Paths.NormalizeAndEvaluate(sourceDirectoryPath), destinationDirectoryPath);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void DeleteDirectoryRecursive(string destinationDirectoryPath)
		{
			var operation = _consoleWriter.BeginDeleteDirectory(destinationDirectoryPath);
			try
			{
				DeleteDirectoryRecursivePrivate(destinationDirectoryPath);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void DeleteFile(string destinationFilePath)
		{
			var operation = _consoleWriter.BeginDeleteFile(destinationFilePath);
			try
			{
				DeleteFilePrivate(destinationFilePath);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
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
			var destinationPath = Path.Combine(Paths.Temp, "DPloy", "Installers");
			var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(installerPath));

			CopyFile(installerPath, destinationFilePath);
			ExecuteFile(destinationFilePath, commandLine ?? "/S");
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
			var operation = _consoleWriter.BeginExecuteCommand(cmd);
			try
			{
				var exitCode = ExecuteCommandPrivate(cmd);
				operation.Success();
				return exitCode;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void StartService(string serviceName)
		{
			var operation = _consoleWriter.BeginStartService(serviceName);
			try
			{
				StartServicePrivate(serviceName);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void StopService(string serviceName)
		{
			var operation = _consoleWriter.BeginStopService(serviceName);
			try
			{
				StopServicePrivate(serviceName);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		#endregion

		private void StartServicePrivate(string serviceName)
		{
			_services.Start(serviceName);
		}

		private void StopServicePrivate(string serviceName)
		{
			_services.Stop(serviceName);
		}

		private void KillProcessesPrivate(string processName)
		{
			_processes.KillAll(processName);
		}

		private int ExecuteCommandPrivate(string cmd)
		{
			return _shell.Execute(cmd);
		}

		private void CopyFilePrivate(string sourceFilePath, string destinationFilePath)
		{
			var fileSize = new FileInfo(sourceFilePath).Length;
			if (IsSmallFile(fileSize))
			{
				CopyFileBatch(new[]{sourceFilePath}, new []{destinationFilePath});
			}
			else
			{
				CopyFileChunked(sourceFilePath, destinationFilePath, fileSize);
			}
		}

		private void CreateDirectoryPrivate(string destinationDirectoryPath)
		{
			_files.CreateDirectoryAsync(destinationDirectoryPath).Wait();
		}

		private void CopyDirectoryRecursivePrivate(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var sourceFiles = new List<string>();
			var destinationFiles = new List<string>();
			foreach (var sourceFilePath in Directory.EnumerateFiles(sourceDirectoryPath, "*.*", SearchOption.AllDirectories))
			{
				sourceFiles.Add(sourceFilePath);

				var relativePath = GetPathRelativeTo(sourceFilePath, sourceDirectoryPath);
				var destinationFilePath = Path.Combine(destinationDirectoryPath, relativePath);

				destinationFiles.Add(destinationFilePath);
			}

			CopyFilesPrivate(sourceFiles, destinationFiles);
		}

		[Pure]
		public static string GetPathRelativeTo(string filePath, string directoryPath)
		{
			Uri fullPath = new Uri(filePath, UriKind.Absolute);

			if (!directoryPath.EndsWith("\\") && !directoryPath.EndsWith("/"))
				directoryPath += '\\';

			Uri relRoot = new Uri(directoryPath, UriKind.Absolute);
			string relativePath = relRoot.MakeRelativeUri(fullPath).ToString();
			return relativePath;
		}

		private void CopyFilesPrivate(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			var destinationFiles = new List<string>();
			foreach (var sourceFile in sourceFiles)
			{
				destinationFiles.Add(Path.Combine(destinationFolder, Path.GetFileName(sourceFile)));
			}

			CopyFilesPrivate(sourceFiles, destinationFiles);
		}

		private void CopyFilesPrivate(IReadOnlyList<string> sourceFiles, IReadOnlyList<string> destinationFiles)
		{
			var tmp = new Dictionary<string, string>();
			for (int i = 0; i < sourceFiles.Count; ++i)
			{
				tmp.Add(sourceFiles[i], destinationFiles[i]);
			}

			var smallFiles = new List<string>();
			var bigFiles = new List<string>();
			ClassifyFilesBySize(sourceFiles, smallFiles, bigFiles);

			foreach (var bigFile in bigFiles)
			{
				var destinationFilePath = tmp[bigFile];
				CopyFilePrivate(bigFile, destinationFilePath);
			}

			var destinationSmallFilePaths = smallFiles.Select(x => tmp[x]).ToList();
			CopyFileBatch(smallFiles, destinationSmallFilePaths);
		}

		private void DeleteDirectoryRecursivePrivate(string destinationDirectoryPath)
		{
			_files.DeleteDirectoryAsync(destinationDirectoryPath, recursive: true).Wait();
		}

		private void DeleteFilePrivate(string destinationFilePath)
		{
			_files.DeleteFileAsync(destinationFilePath).Wait();
		}

		private void CreateFilePrivate(string destinationFilePath, byte[] content)
		{
			var file = new CreateFile
			{
				FilePath = destinationFilePath,
				Content = content
			};
			_files.ExecuteBatchAsync(new FileBatch{FilesToCreate = {file}}).Wait();
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
		/// <param name="destinationFilePath"></param>
		/// <param name="expectedFileSize"></param>
		private void CopyFileChunked(string sourceFilePath, string destinationFilePath, long expectedFileSize)
		{
			var thisHash = HashCodeCalculator.MD5(sourceFilePath);
			if (Exists(destinationFilePath, expectedFileSize, thisHash))
			{
				Log.InfoFormat("Skipping copy of '{0}' to '{1}' because the target file is already present", sourceFilePath,
				               destinationFilePath);
				return;
			}

			Log.InfoFormat("Copying '{0}' to '{1}'...", sourceFilePath, destinationFilePath);

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

		private void CopyFileBatch(IReadOnlyList<string> sourceFilePaths, IReadOnlyList<string> destinationFilePaths)
		{
			var tasks = new List<Task>();

			var batch = new FileBatch();
			long length = 0;
			for(int i = 0; i < sourceFilePaths.Count; ++i)
			{
				var sourceFilePath = sourceFilePaths[i];
				var destinationFilePath = destinationFilePaths[i];

				Log.InfoFormat("Copying '{0}' to '{1}'...", sourceFilePath, destinationFilePath);

				var instruction = new CreateFile
				{
					FilePath = destinationFilePath,
					Content = File.ReadAllBytes(sourceFilePath)
				};
				batch.FilesToCreate.Add(instruction);
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