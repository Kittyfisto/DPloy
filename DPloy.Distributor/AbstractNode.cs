using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DPloy.Core;
using DPloy.Core.PublicApi;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Output;
using log4net;

namespace DPloy.Distributor
{
	internal abstract class AbstractNode
		: INode
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IOperationTracker _operationTracker;
		private readonly IShell _shell;
		private readonly IServices _services;
		private readonly IProcesses _processes;
		private readonly IRegistry _registry;

		protected AbstractNode(IOperationTracker operationTracker,
		                       IShell shell,
		                       IServices services,
		                       IProcesses processes,
		                       IRegistry registry)
		{
			_operationTracker = operationTracker;
			_shell = shell;
			_services = services;
			_processes = processes;
			_registry = registry;
		}

		#region Implementation of IClient

		public int KillProcesses(string processName, string operationName = null)
		{
			var operation = _operationTracker.BeginKillProcesses(new[]{processName}, operationName);
			try
			{
				var numKilled = KillProcessesPrivate(new []{processName});
				operation.Success();
				return numKilled;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public int KillProcesses(string[] processNames, string operationName = null)
		{
			var operation = _operationTracker.BeginKillProcesses(processNames, operationName);
			try
			{
				var numKilled = KillProcessesPrivate(processNames);
				operation.Success();
				return numKilled;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public abstract void DownloadFile(string sourceFileUri, string destinationFilePath);

		public abstract void CreateFile(string destinationFilePath, byte[] content);

		public abstract void CopyFile(string sourceFilePath, string destinationFilePath, bool forceCopy = false);

		public abstract void CopyFiles(IEnumerable<string> sourceFilePaths,
		                               string destinationFolder,
		                               bool forceCopy = false);

		public abstract void DeleteFiles(string wildcardPattern);

		public abstract void CreateDirectory(string destinationDirectoryPath);

		public abstract void CopyDirectory(string sourceDirectoryPath,
		                                   string destinationDirectoryPath,
		                                   bool forceCopy = false);

		public abstract void CopyDirectoryRecursive(string sourceDirectoryPath,
		                                            string destinationDirectoryPath,
		                                            bool forceCopy = false);

		public abstract void DeleteDirectoryRecursive(string destinationDirectoryPath);

		public abstract void DeleteFile(string destinationFilePath);

		public abstract void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false);

		public string GetRegistryStringValue(string keyName, string valueName)
		{
			return _registry.GetStringValue(keyName, valueName);
		}

		public uint GetRegistryDwordValue(string keyName, string valueName)
		{
			return _registry.GetDwordValue(keyName, valueName);
		}

		public void Install(string installerPath, string commandLine = null, bool forceCopy = false)
		{
			var destinationPath = Path.Combine(Paths.Temp, "DPloy", "Installers");
			var destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(installerPath));

			CopyFile(installerPath, destinationFilePath, forceCopy);
			Execute(destinationFilePath, commandLine ?? "/S");
		}

		public void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null, bool printStdOutOnFailure = true, string operationName = null)
		{
			Log.InfoFormat("Executing '{0} {1}'...", clientFilePath, commandLine);
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
			Log.InfoFormat("Executing command '{0}'...", cmd);
			var operation = _operationTracker.BeginExecuteCommand(cmd, operationName);
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

		private int KillProcessesPrivate(string[] processName)
		{
			return _processes.KillAll(processName);
		}

		private int ExecuteCommandPrivate(string cmd)
		{
			return _shell.ExecuteCommand(cmd);
		}
	}
}