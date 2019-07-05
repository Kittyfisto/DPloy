using System;
using System.Collections.Generic;
using System.Net;

namespace DPloy.Distributor.Output
{
	/// <summary>
	///     Forwards all Begin() calls to another <see cref="IOperationTracker" /> except
	///     when a <see cref="IOperationTracker.BeginCustomOperation" /> is executing.
	/// </summary>
	internal sealed class OperationGrouper
		: IOperationTracker
	{
		private readonly IOperationTracker _tracker;
		private GroupedOperation _group;

		public OperationGrouper(IOperationTracker tracker)
		{
			_tracker = tracker;
		}

		public IOperation BeginLoadScript(string scriptFilePath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginLoadScript(scriptFilePath);
		}

		public IOperation BeginCompileScript(string scriptFilePath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCompileScript(scriptFilePath);
		}

		public IOperation BeginConnect(string destination)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginConnect(destination);
		}

		public IOperation BeginDisconnect(IPEndPoint remoteEndPoint)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginDisconnect(remoteEndPoint);
		}

		public IOperation BeginEnumerateFiles(string wildcardPattern)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginEnumerateFiles(wildcardPattern);
		}

		public IOperation BeginFileExists(string fileName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginFileExists(fileName);
		}

		public IOperation BeginCopyFile(string sourcePath, string destinationPath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCopyFile(sourcePath, destinationPath);
		}

		public IOperation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCopyFiles(sourceFiles, destinationFolder);
		}

		public IOperation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
		}

		public IOperation BeginCreateDirectory(string destinationDirectoryPath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCreateDirectory(destinationDirectoryPath);
		}

		public IOperation BeginDeleteDirectory(string destinationDirectoryPath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginDeleteDirectory(destinationDirectoryPath);
		}

		public IOperation BeginDeleteFile(string destinationFilePath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginDeleteFile(destinationFilePath);
		}

		public IOperation BeginDownloadFile(string sourceFileUri, string destinationDirectory)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginDownloadFile(sourceFileUri, destinationDirectory);
		}

		public IOperation BeginCreateFile(string destinationFilePath)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginCreateFile(destinationFilePath);
		}

		public IOperation BeginExecute(string image, string commandLine, string operationName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginExecute(image, commandLine, operationName);
		}

		public IOperation BeginExecuteCommand(string cmd, string operationName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginExecuteCommand(cmd, operationName);
		}

		public IOperation BeginStartService(string serviceName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginStartService(serviceName);
		}

		public IOperation BeginStopService(string serviceName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginStopService(serviceName);
		}

		public IOperation BeginKillProcesses(string[] processNames, string operationName)
		{
			if (_group != null)
				return new DummyOperation();

			return _tracker.BeginKillProcesses(processNames, operationName);
		}

		public IOperation BeginCustomOperation(string operationName)
		{
			if (_group != null)
				throw new NotImplementedException("Custom operations cannot be grouped!");

			var group = _group = new GroupedOperation(this, _tracker.BeginCustomOperation(operationName));
			return group;
		}

		private void Finished(GroupedOperation operation)
		{
			if (_group == operation) _group = null;
		}

		private sealed class GroupedOperation
			: IOperation
		{
			private readonly OperationGrouper _grouper;
			private readonly IOperation _inner;

			public GroupedOperation(OperationGrouper grouper, IOperation inner)
			{
				_grouper = grouper;
				_inner = inner;
			}

			public void Success()
			{
				_grouper.Finished(this);
				_inner.Success();
			}

			public void Failed(Exception exception)
			{
				_grouper.Finished(this);
				_inner.Failed(exception);
			}
		}

		private sealed class DummyOperation
			: IOperation
		{
			public void Success()
			{
			}

			public void Failed(Exception exception)
			{
			}
		}
	}
}