using System.Collections.Generic;
using System.Net;

namespace DPloy.Distributor.Output
{
	/// <summary>
	///     Responsible for keeping track of individual operations (file copy, etc..).
	/// </summary>
	internal interface IOperationTracker
	{
		IOperation BeginLoadScript(string scriptFilePath);
		IOperation BeginCompileScript(string scriptFilePath);
		IOperation BeginConnect(string destination);
		IOperation BeginDisconnect(IPEndPoint remoteEndPoint);
		IOperation BeginEnumerateFiles(string wildcardPattern);
		IOperation BeginFileExists(string fileName);
		IOperation BeginCopyFile(string sourcePath, string destinationPath);
		IOperation BeginCopyFiles(IReadOnlyList<string> sourceFiles, string destinationFolder);
		IOperation BeginCopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath);
		IOperation BeginCreateDirectory(string destinationDirectoryPath);
		IOperation BeginDeleteDirectory(string destinationDirectoryPath);
		IOperation BeginDeleteFile(string destinationFilePath);
		IOperation BeginDownloadFile(string sourceFileUri, string destinationDirectory);
		IOperation BeginCreateFile(string destinationFilePath);
		IOperation BeginExecute(string image, string commandLine, string operationName);
		IOperation BeginExecuteCommand(string cmd, string operationName);
		IOperation BeginStartService(string serviceName);
		IOperation BeginStopService(string serviceName);
		IOperation BeginKillProcesses(string[] processNames, string operationName);
		IOperation BeginCustomOperation(string operationName);
	}
}