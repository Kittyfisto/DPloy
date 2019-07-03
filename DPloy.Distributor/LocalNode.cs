using System;
using System.Collections.Generic;
using System.IO;
using DPloy.Core.SharpRemoteImplementations;
using DPloy.Distributor.Output;
using Registry = DPloy.Core.SharpRemoteImplementations.Registry;

namespace DPloy.Distributor
{
	internal class LocalNode
		: AbstractNode
		, IDisposable
	{
		private readonly IOperationTracker _operationTracker;
		private readonly Files _files;

		public LocalNode(IOperationTracker operationTracker, IFilesystem filesystem)
			: base(operationTracker, new Shell(), new Services(), new Processes(), new Registry())
		{
			_operationTracker = operationTracker;
			_files = new Files(filesystem);
		}

		public override void DownloadFile(string sourceFileUri, string destinationFilePath)
		{
			throw new NotImplementedException();
		}

		public override void CreateFile(string destinationFilePath, byte[] content)
		{
			throw new NotImplementedException();
		}

		public override void CopyFile(string sourceFilePath, string destinationFilePath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void DeleteFile(string destinationFilePath)
		{
			throw new NotImplementedException();
		}

		public override void DeleteFiles(string wildcardPattern)
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

		public override void CreateDirectory(string destinationDirectoryPath)
		{
			throw new NotImplementedException();
		}

		public override void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void DeleteDirectoryRecursive(string destinationDirectoryPath)
		{
			throw new NotImplementedException();
		}

		public override void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{}
	}
}