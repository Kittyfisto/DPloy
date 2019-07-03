using System;
using System.Collections.Generic;
using System.IO;
using DPloy.Core.PublicApi;
using DPloy.Core.SharpRemoteImplementations;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Output;
using Registry = DPloy.Core.SharpRemoteImplementations.Registry;

namespace DPloy.Distributor
{
	internal class LocalNode
		: INode
		, IDisposable
	{
		private readonly IOperationTracker _operationTracker;
		private readonly Shell _shell;
		private readonly Files _files;
		private readonly Registry _registry;

		public LocalNode(IOperationTracker operationTracker, IFilesystem filesystem)
		{
			_operationTracker = operationTracker;
			_shell = new Shell();
			_files = new Files(filesystem);
			_registry = new Registry();
		}

		public void Install(string installerPath, string commandLine = null, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null,
			bool printStdOutOnFailure = true, string operationName = null)
		{
			var operation = _operationTracker.BeginExecute(clientFilePath, commandLine, operationName);
			try
			{
				var output = _shell.StartAndWaitForExit(clientFilePath, commandLine, timeout ?? TimeSpan.FromMilliseconds(-1), printStdOutOnFailure);
				if (output.ExitCode != 0)
					throw new ProcessReturnedErrorException(clientFilePath, output, printStdOutOnFailure);

				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public int ExecuteCommand(string cmd, string operationName = null)
		{
			throw new NotImplementedException();
		}

		public int KillProcesses(string processName, string operationName = null)
		{
			throw new NotImplementedException();
		}

		public int KillProcesses(string[] processNames, string operationName = null)
		{
			throw new NotImplementedException();
		}

		public void DownloadFile(string sourceFileUri, string destinationFilePath)
		{
			throw new NotImplementedException();
		}

		public void CreateFile(string destinationFilePath, byte[] content)
		{
			throw new NotImplementedException();
		}

		public void CopyFile(string sourceFilePath, string destinationFilePath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(string destinationFilePath)
		{
			throw new NotImplementedException();
		}

		public void DeleteFiles(string wildcardPattern)
		{
			var operation = _operationTracker.BeginDeleteFile(wildcardPattern);
			try
			{
				_files.DeleteFilesAsync(wildcardPattern).Wait();
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
			throw new NotImplementedException();
		}

		public void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectoryRecursive(string destinationDirectoryPath)
		{
			throw new NotImplementedException();
		}

		public void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public string GetRegistryStringValue(string keyName, string valueName)
		{
			return _registry.GetStringValue(keyName, valueName);
		}

		public uint GetRegistryDwordValue(string keyName, string valueName)
		{
			return _registry.GetDwordValue(keyName, valueName);
		}

		public void StartService(string serviceName)
		{
			throw new NotImplementedException();
		}

		public void StopService(string serviceName)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{}
	}
}