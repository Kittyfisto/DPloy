using System;
using System.Collections.Generic;

namespace DPloy.Core.PublicApi
{
	/// <summary>
	///     The public interface of a node - a remote computer on which software shall be installed,
	///     files copied, etc...
	/// </summary>
	/// <remarks>
	///     All methods offered by this interface block until either they succeed or an unrecoverable error occured
	///     (such as file not existing, or not writable, etc...)
	/// </remarks>
	/// <remarks>
	///     Parameters such as 'sourceFilePath' refer to a path on the system where the deployment script is executed.
	///     Parameters such as 'destinationFilePath' refer to a path on the node (i.e. the connected remote computer).
	/// </remarks>
	/// <remarks>
	///     TODO: Figure out how a (web)proxy should be configured
	/// </remarks>
	public interface INode
	{
		/// <summary>
		///     Copies the given installer to this client and executes it (using the given command-line).
		/// </summary>
		/// <remarks>
		///     The installer is copied to this node's %temp% folder and is not deleted automatically.
		///     This way subsequent installations (using a binary identical installer) skip the copy file step.
		/// </remarks>
		/// <param name="installerPath"></param>
		/// <param name="commandLine"></param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		void Install(string installerPath, string commandLine = null, bool forceCopy = false);

		/// <summary>
		///     Starts a new process from the given image and waits for the process to exit.
		/// </summary>
		/// <param name="clientFilePath">
		///    A path to an executable on this node's filesystem
		///    <example>%system%\calc.exe</example>
		/// </param>
		/// <param name="commandLine">
		///    The command-line to forward to the newly started process
		///    <example>--bar blub --foo 42</example>
		/// </param>
		/// <param name="timeout">
		///    The amount of time this method should wait for the process to exit,
		///    if no value is defined then it waits for an infinite amount of time.
		/// </param>
		/// <param name="printStdOutOnFailure">
		///    When set to true, then the STDOUT of the started process will be printed
		///    to the console if the process exited with a non-zero value.
		///    When set to false, then the STDOUT of the started process will never be printed.
		/// </param>
		/// <param name="operationName">
		///    The name of this operation which is printed to the console when this step is executing.
		///    If nothing is specified, then DPloy will generate a name based on the parameters, for example
		///    Executing 'cmd.exe /c', however if a non-zero, non-empty string is specified, then DPloy
		///    will use that string instead.
		/// </param>
		/// <param name="showWindow">
		///    When set to true, then another shell window will open which displays all the output of the command
		///    and closes once the command is finished. When set to false, then no additional window will spawn
		///    and the output remains completely hidden. Set to false by default.
		/// </param>
		/// <exception cref="Exception">In case the process exits with a non-zero value</exception>
		void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null, bool printStdOutOnFailure = true, string operationName = null, bool showWindow = false);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="operationName">
		///    The name of this operation which is printed to the console when this step is executing.
		///    If nothing is specified, then DPloy will generate a name based on the parameters, for example
		///    Executing 'cmd.exe /c', however if a non-zero, non-empty string is specified, then DPloy
		///    will use that string instead.
		/// </param>
		/// <param name="showWindow">
		///    When set to true, then another shell window will open which displays all the output of the command
		///    and closes once the command is finished. When set to false, then no additional window will spawn
		///    and the output remains completely hidden. Set to false by default.
		/// </param>
		/// <returns></returns>
		int ExecuteCommand(string cmd, string operationName = null, bool showWindow = false);

		#region Processes

		/// <summary>
		///     Kills all processes with the given name.
		///     Does nothing if there is no process with the given name.
		/// </summary>
		/// <param name="processName">
		///     The name of the process, DOES NOT INCLUDE THE FILE EXTENSION.
		///     <example>explorer</example>
		/// </param>
		/// <param name="operationName">
		///    The name of this operation which is printed to the console when this step is executing.
		///    If nothing is specified, then DPloy will generate a name based on the parameters, for example
		///    Killing 'cmd', however if a non-zero, non-empty string is specified, then DPloy
		///    will use that string instead.
		/// </param>
		/// <returns>The number of processes killed.</returns>
		int KillProcesses(string processName, string operationName = null);

		/// <summary>
		///     Kills all processes with the given names.
		///     Does nothing if there is no process with the given name.
		/// </summary>
		/// <param name="processNames">
		///     The name of the process, DOES NOT INCLUDE THE FILE EXTENSION.
		///     <example>explorer</example>
		/// </param>
		/// <param name="operationName">
		///    The name of this operation which is printed to the console when this step is executing.
		///    If nothing is specified, then DPloy will generate a name based on the parameters, for example
		///    Killing 'cmd', however if a non-zero, non-empty string is specified, then DPloy
		///    will use that string instead.
		/// </param>
		/// <returns>The number of processes killed.</returns>
		int KillProcesses(string[] processNames, string operationName = null);

		#endregion

		#region Networking

		/// <summary>
		///     Downloads a file onto this node.
		/// </summary>
		/// <remarks>
		///     The file is downloaded FROM this node, which means that the node itself must be capable of downloading
		///     the file (i.e. have the appropriate access rights). If this is not the case, then you can simply
		///     call <see cref="System.Net.WebClient.DownloadFile"/> in your script itself, followed by <see cref="CopyFile"/>.
		/// </remarks>
		/// <param name="sourceFileUri"></param>
		/// <param name="destinationFilePath"></param>
		void DownloadFile(string sourceFileUri, string destinationFilePath);

		#endregion

		#region Filesystem

		/// <summary>
		///     Creates a new file on this node.
		///     The content will be written to the new file  and if the file already exists it will be overwritten.
		/// </summary>
		/// <param name="destinationFilePath"></param>
		/// <param name="content"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CreateFile(string destinationFilePath, byte[] content);

		/// <summary>
		///     Copies a single file to this node, blocks until the file has been fully transferred or an error occured.
		/// </summary>
		/// <remarks>
		///     <paramref name="destinationFilePath" /> can be absolute or it may start with a special folder such as:
		///     %temp%, %localappdata%, etc...
		/// </remarks>
		/// <remarks>
		///     If a file with that name already exists on the target system then it will be overwritten.
		/// </remarks>
		/// <example>
		///     <paramref name="sourceFilePath" />: %downloads%\Foo.txt
		///     <paramref name="destinationFilePath" />: %downloads%\Bar.txt
		/// </example>
		/// <param name="sourceFilePath">The file path (relative or absolute) of the file which shall be copied</param>
		/// <param name="destinationFilePath"></param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CopyFile(string sourceFilePath, string destinationFilePath, bool forceCopy = false);

		/// <summary>
		///     Copies the given files *flat* into the given folder.
		/// </summary>
		/// <remarks>
		///     If a file with any of the given names already exists on the target system then it will be overwritten.
		/// </remarks>
		/// <param name="sourceFilePaths">The file path (relative or absolute) of the files which shall be copied</param>
		/// <param name="destinationFolder"></param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CopyFiles(IEnumerable<string> sourceFilePaths, string destinationFolder, bool forceCopy = false);

		/// <summary>
		///     Deletes a file from this node, does nothing if the file doesn't exist.
		/// </summary>
		/// <param name="destinationFilePath"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void DeleteFile(string destinationFilePath);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wildcardPattern"></param>
		void DeleteFiles(string wildcardPattern);

		/// <summary>
		///     Creates a directory on this node if it doesn't already exist.
		/// </summary>
		/// <param name="destinationDirectoryPath"></param>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CreateDirectory(string destinationDirectoryPath);

		/// <summary>
		///     Copies a directory and all of its files (but NOT its sub-directories) to the given
		///     destination folder.
		/// </summary>
		/// <remarks>
		///     Any file already existing at the destination will be overwritten, if need be.
		/// </remarks>
		/// <remarks>
		///     If the source directory is empty, then an empty directory will be created at the destination.
		/// </remarks>
		/// <example>
		///     <paramref name="sourceDirectoryPath" /> is set to C:\MyAwesomeDirectory
		///     <paramref name="destinationDirectoryPath" /> is set to %desktop%\MyNewAwesomeDirectory
		///     This method will create a directory named MyNewAwesomeDirectory on the node's desktop
		///     and place the contents of C:\MyAwesomeDirectory there.
		/// </example>
		/// <param name="sourceDirectoryPath">The path to the source directory which shall be copied</param>
		/// <param name="destinationDirectoryPath">The path on this node where the source directory shall be placed</param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false);

		/// <summary>
		///     Copies a directory and all of its files (including its sub-directories) to the given
		///     destination folder.
		/// </summary>
		/// <remarks>
		///     Any file already existing at the destination will be overwritten, if need be.
		/// </remarks>
		/// <example>
		///     <paramref name="sourceDirectoryPath" /> is set to C:\MyAwesomeDirectory
		///     <paramref name="destinationDirectoryPath" /> is set to %desktop%\MyNewAwesomeDirectory
		///     This method will create a directory named MyNewAwesomeDirectory on the node's desktop
		///     and place the contents of C:\MyAwesomeDirectory there.
		/// </example>
		/// <param name="sourceDirectoryPath">The path to the source directory which shall be copied</param>
		/// <param name="destinationDirectoryPath">The path on this node where the source directory shall be placed</param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void CopyDirectoryRecursive(string sourceDirectoryPath, string destinationDirectoryPath, bool forceCopy = false);

		/// <summary>
		///     Deletes a directory from this node, does nothing if the directory doesn't exist.
		/// </summary>
		/// <param name="destinationDirectoryPath">The directory on the node which shall be deleted</param>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		void DeleteDirectoryRecursive(string destinationDirectoryPath);

		/// <summary>
		///     Copies the given archive to this node and unzips its contents into the given folder.
		/// </summary>
		/// <remarks>
		///     Currently only zip files (*.zip) are supported.
		/// </remarks>
		/// <remarks>
		///     Existing files at the destination folder will be overwritten if they already exist.
		/// </remarks>
		/// <param name="sourceArchivePath">The path to an archive on the distributor's machine</param>
		/// <param name="destinationFolder">A path on the node's machine where the contents of the archive shall be extracted</param>
		/// <param name="forceCopy">When set to true, then the installer will always be copied to this node, even if a binary identical file already exists at the target location.</param>
		void CopyAndUnzipArchive(string sourceArchivePath, string destinationFolder, bool forceCopy = false);

		/// <summary>
		///    Tests if a file exists on this node's file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		bool FileExists(string fileName);

		/// <summary>
		///    Enumerates files on this node's filesystem.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns></returns>
		IEnumerable<string> EnumerateFiles(string folder);

		#endregion

		#region Registry

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <returns></returns>
		string GetRegistryStringValue(string keyName, string valueName);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <returns></returns>
		uint GetRegistryDwordValue(string keyName, string valueName);

		#endregion

		#region Services

		/// <summary>
		///     Starts the given service, does nothing if the service is already running.
		/// </summary>
		/// <exception cref="ArgumentException">In case there is no service named <paramref name="serviceName" /></exception>
		/// <param name="serviceName"></param>
		void StartService(string serviceName);

		/// <summary>
		///     Stops the given service, does nothing if the service is already stopped or if the service doesn't exist.
		/// </summary>
		/// <param name="serviceName"></param>
		void StopService(string serviceName);

		#endregion

		/// <summary>
		///    Executes the given action as a single operatation.
		///    If the action throws, then it is assumed that the operation failed, otherwise
		///    it is assumed that it succeeds.
		/// </summary>
		/// <remarks>
		///    This method allows you to perform any code and have it show up in the execution
		///    log as the given operation. You can even call multiple <see cref="INode"/> methods
		///    and they will show up with the given <paramref name="operationName"/> and not
		///    as individual operations.
		/// </remarks>
		/// <param name="fn"></param>
		/// <param name="operationName"></param>
		void RunCustomOperation(Action<INode> fn, string operationName);

		/// <summary>
		///    Executes the given action as a single operatation.
		///    If the action throws, then it is assumed that the operation failed, otherwise
		///    it is assumed that it succeeds.
		/// </summary>
		/// <remarks>
		///    This method allows you to perform any code and have it show up in the execution
		///    log as the given operation. You can even call multiple <see cref="INode"/> methods
		///    and they will show up with the given <paramref name="operationName"/> and not
		///    as individual operations.
		/// </remarks>
		/// <param name="fn"></param>
		/// <param name="operationName"></param>
		T RunCustomOperation<T>(Func<INode, T> fn, string operationName);
	}
}