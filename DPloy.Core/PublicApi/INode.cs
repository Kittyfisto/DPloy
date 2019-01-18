using System;
using System.Collections.Generic;

namespace DPloy.Core.PublicApi
{
	/// <summary>
	///     The public interface of a node - a remote computer on which software shall be installed,
	///     files copied, etc...
	/// </summary>
	/// <remarks>
	///     TODO: Introduce API methods to copy a folder (recursively) as well as an array of files
	///     TODO: Introduce API methods to download files from a webserver (e.g. the latest build from jenkins, for example)
	///     TODO: Introduce API toggles to force copy files, if desired (maybe these don't need to be performed on a per-method
	///     basis but maybe per node)
	/// </remarks>
	public interface INode : IDisposable
	{
		/// <summary>
		///     Copies the given installer to this client, executes it (using the given command-line) and finally removes the
		///     installer once more.
		/// </summary>
		/// <param name="installerPath"></param>
		/// <param name="commandLine"></param>
		void Install(string installerPath, string commandLine = null);

		void ExecuteFile(string clientFilePath, string commandLine = null);

		int ExecuteCommand(string cmd);

		#region Filesystem

		/// <summary>
		///     Copies a single file to this node, blocks until the file has been fully transferred or an error occured.
		/// </summary>
		/// <remarks>
		///     <paramref name="destinationFolder" /> can be absolute or it may start with a special folder such as:
		///     %temp%, %localappdata%, etc...
		/// </remarks>
		/// <remarks>
		///     If a file with that name already exists on the target system then it will be overwritten.
		/// </remarks>
		/// <param name="sourceFilePath"></param>
		/// <param name="destinationFolder"></param>
		void CopyFile(string sourceFilePath, string destinationFolder);

		/// <summary>
		///     Copies the given files into the given folder.
		/// </summary>
		/// <remarks>
		///     If a file with any of the given names already exists on the target system then it will be overwritten.
		/// </remarks>
		/// <param name="sourceFiles"></param>
		/// <param name="destinationFolder"></param>
		void CopyFiles(IEnumerable<string> sourceFiles, string destinationFolder);

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
		void CopyAndUnzipArchive(string sourceArchivePath,
		                         string destinationFolder);

		#endregion

		#region Services

		/// <summary>
		///     Starts the given service, does nothing if the service is already running.
		/// </summary>
		/// <exception cref="ArgumentException">In case there is no service named <paramref name="serviceName" /></exception>
		/// <param name="serviceName"></param>
		void StartService(string serviceName);

		/// <summary>
		///     Stops the given service, does nothing if the service is already stopped.
		/// </summary>
		/// <exception cref="ArgumentException">In case there is no service named <paramref name="serviceName" /></exception>
		/// <param name="serviceName"></param>
		void StopService(string serviceName);

		#endregion
	}
}