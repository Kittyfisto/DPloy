// This is an example deployment script (using cs-script)
using DPloy.Core.PublicApi;

public class Deployment
{
	/// <summary>
	///     The main deployment entry point of this script.
	///     This method is called by Distributor.exe
	///     and is meant to contain the code to deploy software on remote machines.
	/// </summary>
	/// <param name="node">
	///     A reference to a remote node. It offers methods such as:
	/// 
	///     - void Install(string installerPath, string commandLine = null);
	///     - void ExecuteFile(string clientFilePath, string commandLine = null);
	///     - int ExecuteCommand(string cmd);
	///
	///     - void KillProcesses(string processName);
	/// 
	///     - void CreateFile(string destinationFilePath, byte[] content);
	///     - void CopyFile(string sourceFilePath, string destinationFilePath)
	///     - void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder);
	///     - void DeleteFile(string destinationFilePath);
	///     - void CreateDirectory(string destinationDirectoryPath);
	///     - void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath);
	///     - void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath);
	///     - void DeleteDirectoryRecursive(string destinationDirectoryPath);
	///     - void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder);
	///
	///     - void StartService(string serviceName);
	///     - void StopService(string serviceName);
	/// </param>
	public static void Deploy(INode node)
	{
		// Copying files is done by specifying the source file path and the destination *directory*.
		// The source file path always refers to a file path on the local machine (where Distributor.exe is running)
		// The destination file path always refers to a file path on the remote node (where Node.exe is running)
		node.CopyFile("HelloWorld.cs", @"%temp%\");
	}
}
