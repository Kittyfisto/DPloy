using System;
using System.Collections.Generic;
using System.Net;

namespace DPloy.Distributor.Output
{
	/// <summary>
	/// Keeps track of individual operations.
	/// </summary>
	internal sealed class OperationTracker
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
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCompileScript(string scriptFilePath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginConnect(string destination)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginDisconnect(IPEndPoint remoteEndPoint)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCopyFile(string sourcePath, string destinationPath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCreateDirectory(string destinationDirectoryPath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginDeleteDirectory(string destinationDirectoryPath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginDeleteFile(string destinationFilePath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginDownloadFile(string sourceFileUri, string destinationDirectory)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginCreateFile(string destinationFilePath)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginExecuteCommand(string cmd)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginStartService(string serviceName)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginStopService(string serviceName)
		{
			var operation = new Operation();
			_operations.Add(operation);
			return operation;
		}

		public IOperation BeginKillProcesses(string processName)
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