using System;
using System.Collections.Generic;
using System.IO;
using DPloy.Core;
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
		private readonly IFilesystem _filesystem;

		public LocalNode(IOperationTracker operationTracker, IFilesystem filesystem)
			: base(operationTracker, new Files(filesystem), new Shell(), new Services(), new Processes(), new Registry())
		{
			_operationTracker = operationTracker;
			_filesystem = filesystem;
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

		public override void DeleteFiles(string wildcardPattern)
		{
			var operation = _operationTracker.BeginDeleteFile(wildcardPattern);
			try
			{
				foreach (var file in EnumerateFilesPrivate(wildcardPattern))
				{
					_filesystem.DeleteFile(file);
				}
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public override void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<string> EnumerateFiles(string wildcardPattern)
		{
			var operation = _operationTracker.BeginEnumerateFiles(wildcardPattern);
			try
			{
				var files = EnumerateFilesPrivate(wildcardPattern);
				operation.Success();
				return files;
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		private IReadOnlyList<string> EnumerateFilesPrivate(string wildcardPattern)
		{
			var path = Path.GetDirectoryName(wildcardPattern);
			var fileName = Path.GetFileName(wildcardPattern);
			var absolutePath = Core.Paths.NormalizeAndEvaluate(path, _filesystem.CurrentDirectory);
			var files = _filesystem.EnumerateFiles(absolutePath, fileName, SearchOption.TopDirectoryOnly,
				tolerateNonExistantPath: true);
			return files;
		}

		public void Dispose()
		{}
	}
}