using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using log4net;

namespace DPloy.Distributor.Output
{
	/// <summary>
	///     Writes the progress of individual operations immediately to the console.
	/// </summary>
	internal sealed class ConsoleWriter
		: IOperationTracker
	{
		private const string CutIndicator = "[...]";

		/// <summary>
		///     Indentation used for operations running on a particular node.
		/// </summary>
		private const string NodeOperationIndent = "  ";

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly int _maxStatusWidth;
		private readonly bool _verbose;

		public ConsoleWriter(bool verbose)
		{
			_verbose = verbose;
			_maxStatusWidth = Math.Max(ConsoleWriterOperation.Ok.Length, ConsoleWriterOperation.Error.Length);
		}

		private int MaxLineLength
		{
			get
			{
				try
				{
					return Console.WindowWidth - _maxStatusWidth - 1;
				}
				catch (Exception)
				{
					// Console.WidthWidth fails in unit test scenarios
					return 120 - _maxStatusWidth;
				}
			}
		}

		public IOperation BeginLoadScript(string scriptFilePath)
		{
			var template = "Loading script '{0}'";
			var maxLineLength = MaxLineLength;
			var remaminingLength = maxLineLength - template.Length + 3;
			var pruntedScriptFilePath = PrunePath(scriptFilePath, remaminingLength);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedScriptFilePath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCompileScript(string scriptFilePath)
		{
			var template = "Compiling script '{0}'";
			var maxLineLength = MaxLineLength;
			var remaminingLength = maxLineLength - template.Length + 3;
			var pruntedScriptFilePath = PrunePath(scriptFilePath, remaminingLength);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedScriptFilePath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginConnect(string destination)
		{
			var template = "Connecting to '{0}'";
			var maxLineLength = MaxLineLength;
			var message = new StringBuilder();
			message.AppendFormat(template, destination);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginDisconnect(IPEndPoint remoteEndPoint)
		{
			var template = "Disconnecting from '{0}'";
			var maxLineLength = MaxLineLength;
			var message = new StringBuilder();
			message.AppendFormat(template, remoteEndPoint);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCopyFile(string sourcePath, string destinationPath)
		{
			var template = NodeOperationIndent + "Copying '{0}' to '{1}'";
			var maxLineLength = MaxLineLength;
			var remamining = maxLineLength - template.Length + 2 * 3;

			PruneTwoPaths(sourcePath, destinationPath, remamining,
			              out var prunedSourcePath,
			              out var pruntedDestinationPath);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedSourcePath, pruntedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder)
		{
			var template = NodeOperationIndent + $"Copying {sourceFiles.Count} files to " + "'{0}'";
			var maxLineLength = MaxLineLength;
			var remamining = maxLineLength - template.Length + 2 * 3;

			var pruntedDestinationPath = PrunePath(destinationFolder, remamining);

			var message = new StringBuilder();
			message.AppendFormat(template, pruntedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
		{
			var template = NodeOperationIndent + "Copying directory '{0}' to '{1}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 2 * 3;

			PruneTwoPaths(sourceDirectoryPath, destinationDirectoryPath, remaining,
			              out var prunedSourcePath,
			              out var prunedDestinationPath);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedSourcePath, prunedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCreateDirectory(string destinationDirectoryPath)
		{
			var template = NodeOperationIndent + "Creating directory '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedDestinationPath = PrunePath(destinationDirectoryPath, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginDeleteDirectory(string destinationDirectoryPath)
		{
			var template = NodeOperationIndent + "Deleting directory '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedDestinationPath = PrunePath(destinationDirectoryPath, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginDeleteFile(string destinationFilePath)
		{
			var template = NodeOperationIndent + "Deleting file '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedDestinationPath = PrunePath(destinationFilePath, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginDownloadFile(string sourceFileUri, string destinationDirectory)
		{
			var template = NodeOperationIndent + "Downloading '{0}' to '{1}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 2 * 3;

			PruneTwoPaths(sourceFileUri, destinationDirectory,
			              remaining,
			              out var prunedSourceFile,
			              out var prunedDestinationDirectory);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedSourceFile, prunedDestinationDirectory);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginCreateFile(string destinationFilePath)
		{
			var template = NodeOperationIndent + "Creating file '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedDestinationPath = PrunePath(destinationFilePath, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedDestinationPath);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginExecute(string image, string commandLine)
		{
			var template = NodeOperationIndent + "Executing '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var cmd = string.IsNullOrEmpty(commandLine)
				? image
				: $"{image} {commandLine}";
			var prunedCommand = PruneEnd(cmd, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedCommand);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginExecuteCommand(string cmd)
		{
			var template = NodeOperationIndent + "Executing '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedCommand = PruneEnd(cmd, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedCommand);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginStartService(string serviceName)
		{
			Log.InfoFormat("Starting service '{0}'...", serviceName);

			var template = NodeOperationIndent + "Starting service '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedServiceName = PruneEnd(serviceName, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedServiceName);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginStopService(string serviceName)
		{
			Log.InfoFormat("Stopping service '{0}'...", serviceName);

			var template = NodeOperationIndent + "Stopping service '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedServiceName = PruneEnd(serviceName, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedServiceName);
			return CreateOperation(message, maxLineLength);
		}

		public IOperation BeginKillProcesses(string processName)
		{
			Log.InfoFormat("Killing process(es) '{0}'...", processName);

			var template = NodeOperationIndent + "Killing process(es) '{0}'";
			var maxLineLength = MaxLineLength;
			var remaining = maxLineLength - template.Length + 3;

			var prunedServiceName = PruneEnd(processName, remaining);

			var message = new StringBuilder();
			message.AppendFormat(template, prunedServiceName);
			return CreateOperation(message, maxLineLength);
		}

		private ConsoleWriterOperation CreateOperation(StringBuilder message, int maxLineLength)
		{
			return CreateOperation(message.ToString(), maxLineLength);
		}

		private ConsoleWriterOperation CreateOperation(string message, int maxLineLength)
		{
			return new ConsoleWriterOperation(Console.Out, message, maxLineLength, _verbose);
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

		private static string PruneEnd(string cmd, int remaining)
		{
			if (cmd.Length > remaining)
			{
				var builder = new StringBuilder(remaining);
				builder.Append(cmd, startIndex: 0, count: remaining - CutIndicator.Length);
				builder.Append(CutIndicator);
				return builder.ToString();
			}

			return cmd;
		}

		public static string PrunePath(string path, int maxLength)
		{
			if (path.Length > maxLength)
			{
				var charactersToCut = path.Length - maxLength + CutIndicator.Length;

				var startCut = FindIdealCutPoint(path, charactersToCut);
				var endCut = startCut + charactersToCut;

				var builder = new StringBuilder(maxLength);
				builder.Append(path, startIndex: 0, count: startCut);
				builder.Append(CutIndicator);
				builder.Append(path, endCut, path.Length - endCut);

				return builder.ToString();
			}

			return path;
		}

		private static int FindIdealCutPoint(string path, int charactersToCut)
		{
			// We've decided that we need to cut 'charactersToCut' amount of characters from that path.
			// We assume that the filename is important so it won't be cut unless absolutely necessary!
			var fileName = Path.GetFileName(path);
			var remaining = path.Length - fileName.Length - 1;
			if (remaining >= charactersToCut)
			{
				var startCut = path.Length - fileName.Length - charactersToCut - 1;
				return startCut;
			}
			else
			{
				var halfTooMuch = charactersToCut / 2;
				var startCut = path.Length / 2 - halfTooMuch;
				return startCut;
			}
		}
	}
}