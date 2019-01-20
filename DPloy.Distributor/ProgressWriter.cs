using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DPloy.Distributor
{
	/// <summary>
	/// </summary>
	internal sealed class ProgressWriter
	{
		private readonly bool _verbose;
		private const string CutIndicator = "[...]";

		private readonly int _maxStatusWidth;
		private readonly int _maxOperationWidth;

		public ProgressWriter(bool verbose)
		{
			_verbose = verbose;
			_maxStatusWidth = Math.Max(Operation.Ok.Length, Operation.Error.Length);

			try
			{
				_maxOperationWidth = Console.WindowWidth - _maxStatusWidth - 1;
			}
			catch (Exception)
			{
				// Console.WidthWidth fails in unit test scenarios
				_maxOperationWidth = 120 - _maxStatusWidth;
			}
		}

		public Operation BeginLoadScript(string scriptFilePath)
		{
			var template = "Loading script '{0}'";
			var remaminingLength = _maxOperationWidth - template.Length + 3;
			var pruntedScriptFilePath = PrunePath(scriptFilePath, remaminingLength);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedScriptFilePath);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginCompileScript(string scriptFilePath)
		{
			var template = "Compiling script '{0}'";
			var remaminingLength = _maxOperationWidth - template.Length + 3;
			var pruntedScriptFilePath = PrunePath(scriptFilePath, remaminingLength);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedScriptFilePath);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginConnect(string destination)
		{
			var template = "Connecting to '{0}'S";
			var message = new StringBuilder();
			message.AppendFormat(template, destination);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginDisconnect(IPEndPoint remoteEndPoint)
		{
			var template = "Disconnecting from '{0}'";
			var message = new StringBuilder();
			message.AppendFormat(template, remoteEndPoint);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginCopyFile(string sourcePath, string destinationPath)
		{
			var template = "Copying '{0}' to '{1}'";
			var remamining = _maxOperationWidth - template.Length + 2 * 3;

			PruneTwoPaths(sourcePath, destinationPath, remamining,
			              out var prunedSourcePath,
			              out var pruntedDestinationPath);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedSourcePath, pruntedDestinationPath);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			var template = $"Copying {sourceFiles.Count} files to " + "'{0}'";
			var remamining = _maxOperationWidth - template.Length + 2 * 3;

			var pruntedDestinationPath = PrunePath(destinationFolder, remamining);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedDestinationPath);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		public Operation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var template = "Copying directory '{0}' to '{1}'";
			var remamining = _maxOperationWidth - template.Length + 2 * 3;

			PruneTwoPaths(sourceDirectoryPath, destinationDirectoryPath, remamining,
			              out var prunedSourcePath,
			              out var pruntedDestinationPath);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedSourcePath, pruntedDestinationPath);
			FillIfNecessary(message);
			return new Operation(message.ToString(), _verbose);
		}

		private void FillIfNecessary(StringBuilder message)
		{
			if (message.Length < _maxOperationWidth)
				message.Append(value: ' ', repeatCount: _maxOperationWidth - message.Length);
		}

		private static void PruneTwoPaths(string path1,
		                           string path2,
		                           int maxWidth,
		                           out string prunedPath1,
		                           out string prunedPath2)
		{
			if (path1.Length + path2.Length <= maxWidth)
			{
				prunedPath1 = path1;
				prunedPath2 = path2;
			}
			else
			{
				var path1MaxLength = maxWidth / 2;
				var path2MaxLength = maxWidth - path1MaxLength;

				prunedPath1 = PrunePath(path1, path1MaxLength);
				prunedPath2 = PrunePath(path2, path2MaxLength);
			}
		}

		private static string PrunePath(string path, int maxLength)
		{
			if (path.Length > maxLength)
			{
				var cutIndicator = "[...]";
				var tooMuch = path.Length - maxLength - CutIndicator.Length;
				var halfTooMuch = tooMuch / 2;
				var startCut = path.Length / 2 - halfTooMuch;
				var endCut = startCut + tooMuch - halfTooMuch;

				var builder = new StringBuilder(maxLength);
				builder.Append(path, startIndex: 0, count: startCut);
				builder.Append(CutIndicator);
				builder.Append(path, endCut, path.Length - endCut);

				return builder.ToString();
			}

			return path;
		}
	}
}