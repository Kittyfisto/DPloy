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
using DPloy.Distributor.Output;
using DPloy.Distributor.SharpRemoteImplementations;
using log4net;
using SharpRemote;
using NotConnectedException = DPloy.Distributor.Exceptions.NotConnectedException;

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

		private readonly IOperationTracker _operationTracker;
		private readonly IFiles _files;
		private readonly IShell _shell;
		private readonly IServices _services;
		private readonly IProcesses _processes;
		private readonly INetwork _network;
		private readonly SocketEndPoint _socket;

		private const int FilePacketBufferSize = 1024 * 1024 * 4;
		private const int MaxParallelBatchTasks = 20;
		private const int MaxParallelCopyTasks = 20;

		private NodeClient(IOperationTracker operationTracker, SocketEndPoint socket, string remoteMachineName)
		{
			_operationTracker = operationTracker;
			_socket = socket;

			ThrowIfIncompatible(socket);

			_files = new FilesWrapper(_socket.GetExistingOrCreateNewProxy<IFiles>(ObjectIds.File),
			                          remoteMachineName);
			_shell = new ShellWrapper(_socket.GetExistingOrCreateNewProxy<IShell>(ObjectIds.Shell),
			                          remoteMachineName);
			_services = new ServicesWrapper(_socket.GetExistingOrCreateNewProxy<IServices>(ObjectIds.Services),
			                                remoteMachineName);
			_processes = new ProcessesWrapper(_socket.GetExistingOrCreateNewProxy<IProcesses>(ObjectIds.Processes),
			                                  remoteMachineName);
			_network = new NetworkWrapper(_socket.GetExistingOrCreateNewProxy<INetwork>(ObjectIds.Network),
			                                  remoteMachineName);
		}

		private static void ThrowIfIncompatible(SocketEndPoint socket)
		{
			var expectedInterfaces = new[]
				{typeof(IFiles), typeof(IShell), typeof(IServices), typeof(IProcesses), typeof(INetwork)};

			var interfaces = socket.GetExistingOrCreateNewProxy<IInterfaces>(ObjectIds.Interface);
			var actualTypeModel = interfaces.GetTypeModel();
			actualTypeModel.TryResolveTypes();
			foreach (var expectedInterface in expectedInterfaces)
			{
				ThrowIfIncompatible(expectedInterface, actualTypeModel);
			}
		}

		private static void ThrowIfIncompatible(Type expectedInterface, TypeModel actualTypeModel)
		{
			var expectedTypeModel = new TypeModel();
			var expectedDescription = expectedTypeModel.Add(expectedInterface, assumeByReference: true);

			var actualDescription = actualTypeModel.Types.FirstOrDefault(x => x.AssemblyQualifiedName == expectedInterface.AssemblyQualifiedName);
			if (actualDescription == null)
				throw new NotImplementedException($"The remote is missing interface: {expectedInterface.Name}");

			ThrowIfIncompatible(expectedDescription, actualDescription);
		}

		private static void ThrowIfIncompatible(ITypeDescription expectedDescription, TypeDescription actualDescription)
		{
			foreach (var expectedMethod in expectedDescription.Methods)
			{
				var actualMethod = actualDescription.Methods.FirstOrDefault(x => x.Name == expectedMethod.Name);
				if (actualMethod == null)
					throw new NotImplementedException($"The remote is missing interface method: {expectedDescription.Type.Name}.{expectedMethod.Name}");

				ThrowIfIncompatible(expectedMethod, actualMethod);
			}
		}

		private static void ThrowIfIncompatible(IMethodDescription expectedDescription, MethodDescription actualDescription)
		{
			ThrowIfIncompatible(expectedDescription.ReturnParameter, actualDescription.ReturnParameter);

			if (expectedDescription.Parameters.Count != actualDescription.Parameters.Length)
				throw new NotImplementedException($"The remote method has a different amount of parameters");

			for(int i = 0; i < expectedDescription.Parameters.Count; ++i)
			{
				var expectedParameter = expectedDescription.Parameters[i];
				var actualParameter = actualDescription.Parameters[i];
				ThrowIfIncompatible(expectedParameter, actualParameter);
			}
		}

		private static void ThrowIfIncompatible(IParameterDescription expectedParameter, ParameterDescription actualParameter)
		{
			ThrowIfIncompatible(expectedParameter.ParameterType, actualParameter.ParameterType);
		}

		#region IDisposable

		public void Dispose()
		{
			var socket = _socket;
			if (socket != null && !socket.IsDisposed)
			{
				var operation = _operationTracker.BeginDisconnect(socket.RemoteEndPoint);
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

		public static NodeClient Create(IOperationTracker operationTracker, IPEndPoint endPoint)
		{
			var operation = operationTracker.BeginConnect(endPoint.ToString());

			try
			{
				var node = CreatePrivate(operationTracker, endPoint);
				operation.Success();
				return node;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public static NodeClient Create(IOperationTracker operationTracker, string address, TimeSpan connectTimeout)
		{
			var operation = operationTracker.BeginConnect(address);

			try
			{
				var node = CreatePrivate(operationTracker, address, connectTimeout);

				operation.Success();
				return node;
			}
			catch (SharpRemoteException e)
			{
				operation.Failed(e);
				throw new NotConnectedException(e);
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		private static NodeClient CreatePrivate(IOperationTracker operationTracker,
		                                        string address,
		                                        TimeSpan connectTimeout)
		{
			var socket = new SocketEndPoint(EndPointType.Client, "Distributor",
				clientAuthenticator: MachineNameAuthenticator.CreateClient(),
				heartbeatSettings: new HeartbeatSettings
				{
					AllowRemoteHeartbeatDisable = true
				});

			try
			{
				Connect(socket, address, connectTimeout);
				return new NodeClient(operationTracker, socket, address);
			}
			catch (Exception)
			{
				socket.Dispose();
				throw;
			}
		}

		private static void Connect(SocketEndPoint socket, string address, TimeSpan connectTimeout)
		{
			if (TryParseIPEndPoint(address, out var ipEndPoint))
			{
				socket.Connect(ipEndPoint, connectTimeout);
			}
			else if (IPAddress.TryParse(address, out var ipAddress))
			{
				socket.Connect(new IPEndPoint(ipAddress, Constants.ConnectionPort), connectTimeout);
			}
			else
			{
				var addresses = Dns.GetHostAddresses(address);
				if (addresses == null || addresses.Length == 0)
					throw new ArgumentException($"Unable to resolve '{address}' to an IPAddress - is this machine reachable?");

				socket.Connect(new IPEndPoint(addresses[0], Constants.ConnectionPort));
			}
		}

		private static bool TryParseIPEndPoint(string address, out IPEndPoint ipEndPoint)
		{
			int idx = address.IndexOf(':');
			if (idx == -1)
			{
				ipEndPoint = null;
				return false;
			}

			var ipPart = address.Substring(0, idx);
			var portPart = address.Substring(idx + 1);
			if (!IPAddress.TryParse(ipPart, out var ipAddress))
			{
				ipEndPoint = null;
				return false;
			}

			if (!int.TryParse(portPart, out var port))
			{
				ipEndPoint = null;
				return false;
			}

			ipEndPoint = new IPEndPoint(ipAddress, port);
			return true;
		}

		private static NodeClient CreatePrivate(IOperationTracker operationTracker, IPEndPoint endPoint)
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
				return new NodeClient(operationTracker, socket, endPoint.ToString());
			}
			catch (Exception)
			{
				socket.Dispose();
				throw;
			}
		}

		#region Implementation of IClient

		public int KillProcesses(string processName)
		{
			var operation = _operationTracker.BeginKillProcesses(processName);
			try
			{
				var numKilled = KillProcessesPrivate(processName);
				operation.Success();
				return numKilled;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void DownloadFile(string sourceFileUri, string destinationFilePath)
		{
			var operation = _operationTracker.BeginDownloadFile(sourceFileUri, destinationFilePath);
			try
			{
				DownloadFilePrivate(sourceFileUri, destinationFilePath);
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
			var operation = _operationTracker.BeginCreateFile(destinationFilePath);
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

		public void CopyFile(string sourceFilePath, string destinationFilePath, bool forceCopy = false)
		{
			var operation = _operationTracker.BeginCopyFile(sourceFilePath, destinationFilePath);
			try
			{
				CopyFilePrivate(Paths.NormalizeAndEvaluate(sourceFilePath), destinationFilePath, forceCopy);
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder, bool forceCopy = false)
		{
			var operation = _operationTracker.BeginCopyFiles(sourceFilePaths.ToList(), destinationFolder);
			try
			{
				CopyFilesPrivate(sourceFilePaths.Select(Paths.NormalizeAndEvaluate).ToArray(), destinationFolder, forceCopy);
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
			var operation = _operationTracker.BeginCreateDirectory(destinationDirectoryPath);
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

		public void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			var operation = _operationTracker.BeginCopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
			try
			{
				var sourceFiles = Directory.EnumerateFiles(Paths.NormalizeAndEvaluate(sourceDirectoryPath)).ToList();
				if (sourceFiles.Any())
				{
					CopyFilesPrivate(sourceFiles, destinationDirectoryPath, forceCopy);
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

		public void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			var operation = _operationTracker.BeginCopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
			try
			{
				CopyDirectoryRecursivePrivate(Paths.NormalizeAndEvaluate(sourceDirectoryPath), destinationDirectoryPath, forceCopy);
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
			var operation = _operationTracker.BeginDeleteDirectory(destinationDirectoryPath);
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
			var operation = _operationTracker.BeginDeleteFile(destinationFilePath);
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

		public void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false)
		{
			var destinationArchiveFolder = @"%temp%";
			CopyFile(sourceArchivePath, destinationArchiveFolder);

			UnzipArchive(sourceArchivePath, destinationFolder, destinationArchiveFolder, overwrite: true);
		}

		public void Install(string installerPath, string commandLine = null, bool forceCopy = false)
		{
			var destinationPath = Path.Combine(Paths.Temp, "DPloy", "Installers");
			var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(installerPath));

			CopyFile(installerPath, destinationFilePath, forceCopy);
			Execute(destinationFilePath, commandLine ?? "/S");
		}

		public void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null)
		{
			Log.InfoFormat("Executing '{0} {1}'...", clientFilePath, commandLine);
			var operation = _operationTracker.BeginExecuteCommand(clientFilePath);
			try
			{
				var exitCode = _shell.StartAndWaitForExit(clientFilePath, commandLine, timeout ??  TimeSpan.FromMilliseconds(-1));
				if (exitCode != 0)
					throw new Exception($"{clientFilePath} returned {exitCode}");

				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public int ExecuteCommand(string cmd)
		{
			Log.InfoFormat("Executing command '{0}'...", cmd);
			var operation = _operationTracker.BeginExecuteCommand(cmd);
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
			var operation = _operationTracker.BeginStartService(serviceName);
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
			var operation = _operationTracker.BeginStopService(serviceName);
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

		private int KillProcessesPrivate(string processName)
		{
			return _processes.KillAll(processName);
		}

		private int ExecuteCommandPrivate(string cmd)
		{
			return _shell.ExecuteCommand(cmd);
		}

		private void UnzipArchive(string sourceArchivePath, string destinationFolder, string tmpFolder, bool overwrite)
		{
			Log.InfoFormat("Unzipping '{0}' into '{1}'", sourceArchivePath, destinationFolder);

			var destinationArchivePath = Path.Combine(tmpFolder, Path.GetFileName(sourceArchivePath));
			_files.Unzip(destinationArchivePath, destinationFolder, overwrite);
		}

		private void CopyFilePrivate(string sourceFilePath, string destinationFilePath, bool forceCopy)
		{
			var fileSize = new FileInfo(sourceFilePath).Length;
			if (IsSmallFile(fileSize))
			{
				CopyFileBatch(new[]{sourceFilePath}, new []{destinationFilePath});
			}
			else
			{
				CopyFileChunked(sourceFilePath, destinationFilePath, fileSize, forceCopy);
			}
		}

		private void CreateDirectoryPrivate(string destinationDirectoryPath)
		{
			_files.CreateDirectoryAsync(destinationDirectoryPath).Wait();
		}

		private void CopyDirectoryRecursivePrivate(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy)
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

			CopyFilesPrivate(sourceFiles, destinationFiles, forceCopy);
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

		private void CopyFilesPrivate(IReadOnlyList<string> sourceFiles, string destinationFolder, bool forceCopy)
		{
			var destinationFiles = new List<string>();
			foreach (var sourceFile in sourceFiles)
			{
				destinationFiles.Add(Path.Combine(destinationFolder, Path.GetFileName(sourceFile)));
			}

			CopyFilesPrivate(sourceFiles, destinationFiles, forceCopy);
		}

		private void CopyFilesPrivate(IReadOnlyList<string> sourceFiles, IReadOnlyList<string> destinationFiles, bool forceCopy)
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
				CopyFilePrivate(bigFile, destinationFilePath, forceCopy);
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

		private void DownloadFilePrivate(string sourceFileUri, string destinationDirectory)
		{
			_network.DownloadFileAsync(sourceFileUri, destinationDirectory).Wait();
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
		private void CopyFileChunked(string sourceFilePath, string destinationFilePath, long expectedFileSize, bool forceCopy)
		{
			var thisHash = HashCodeCalculator.MD5(sourceFilePath);
			if (!forceCopy && Exists(destinationFilePath, expectedFileSize, thisHash))
			{
				Log.InfoFormat("Skipping copy of '{0}' to '{1}' because the target file is already present", sourceFilePath,
				               destinationFilePath);
				return;
			}

			Log.InfoFormat("Copying '{0}' to '{1}'...", sourceFilePath, destinationFilePath);

			using (var sourceStream = File.OpenRead(sourceFilePath))
			{
				var fileSize = sourceStream.Length;
				var tasks = new TaskList(maxPending: MaxParallelCopyTasks);
				tasks.Add(_files.OpenFileAsync(destinationFilePath, fileSize));

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

				tasks.WaitAll();
				var clientHash = _files.CalculateMD5(destinationFilePath);
				if (!HashCodeCalculator.AreEqual(thisHash, clientHash))
					throw new
						NotImplementedException($"There has been an error while copying '{sourceFilePath}' to '{destinationFilePath}', the resulting hashes should be identical, but are not!");
			}
		}

		private void CopyFileBatch(IReadOnlyList<string> sourceFilePaths, IReadOnlyList<string> destinationFilePaths)
		{
			var tasks = new TaskList(maxPending: MaxParallelBatchTasks);

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

			tasks.WaitAll();
		}
	}
}