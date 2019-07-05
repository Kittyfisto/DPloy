using System;
using System.Collections.Generic;
using System.Net;

namespace DPloy.Distributor.Output
{
	/// <summary>
	/// Keeps track of individual operations.
	/// </summary>
	public sealed class OperationTracker
		: IOperationTracker
	{
		private readonly List<Operation> _operations;

		public OperationTracker()
		{
			_operations = new List<Operation>();
		}

		#region Implementation of IConsoleWriter

		public IOperation BeginLoadScript(string scriptFilePath)
		{
			return AddOperation();
		}

		public IOperation BeginCompileScript(string scriptFilePath)
		{
			return AddOperation();
		}

		public IOperation BeginConnect(string destination)
		{
			return AddOperation();
		}

		public IOperation BeginDisconnect(IPEndPoint remoteEndPoint)
		{
			return AddOperation();
		}

		public IOperation BeginEnumerateFiles(string wildcardPattern)
		{
			return AddOperation();
		}

		public IOperation BeginFileExists(string fileName)
		{
			return AddOperation();
		}

		public IOperation BeginCopyFile(string sourcePath, string destinationPath)
		{
			return AddOperation();
		}

		public IOperation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			return AddOperation();
		}

		public IOperation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			return AddOperation();
		}

		public IOperation BeginCreateDirectory(string destinationDirectoryPath)
		{
			return AddOperation();
		}

		public IOperation BeginDeleteDirectory(string destinationDirectoryPath)
		{
			return AddOperation();
		}

		public IOperation BeginDeleteFile(string destinationFilePath)
		{
			return AddOperation();
		}

		public IOperation BeginDownloadFile(string sourceFileUri, string destinationDirectory)
		{
			return AddOperation();
		}

		public IOperation BeginCreateFile(string destinationFilePath)
		{
			return AddOperation();
		}

		public IOperation BeginExecute(string image, string commandLine, string operationName)
		{
			return AddOperation();
		}

		public IOperation BeginExecuteCommand(string cmd, string operationName)
		{
			return AddOperation();
		}

		public IOperation BeginStartService(string serviceName)
		{
			return AddOperation();
		}

		public IOperation BeginStopService(string serviceName)
		{
			return AddOperation();
		}

		public IOperation BeginKillProcesses(string[] processNames, string operationName)
		{
			return AddOperation();
		}

		public IOperation BeginCustomOperation(string operationName)
		{
			return AddOperation();
		}

		private IOperation AddOperation()
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		#endregion

		public void ThrowOnFailure()
		{
			foreach (var operation in _operations)
			{
				operation.ThrowOnFailure();
			}
		}

		public void Success()
		{}

		public void Failed(Exception exception)
		{}
	}
}