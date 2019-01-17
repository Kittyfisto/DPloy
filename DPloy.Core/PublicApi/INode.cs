using System;
using System.Collections.Generic;

namespace DPloy.Core.PublicApi
{
	/// <summary>
	///     The public interface of a node - a remote computer on which software shall be installed,
	///     files copied, etc...
	/// </summary>
	/// <remarks>
	/// TODO: Introduce API methods to copy a folder (recursively) as well as an array of files
	/// TODO: Introduce API methods to download files from a webserver (e.g. the latest build from jenkins, for example)
	/// TODO: Introduce API toggles to force copy files, if desired (maybe these don't need to be performed on a per-method basis but maybe per node)
	/// </remarks>
	public interface INode : IDisposable
	{
		#region Filesystem

		/// <summary>
		///     Copies a single file to this node, blocks until the file has been fully transfered or an error occured.
		/// </summary>
		/// <remarks>
		///     <paramref name="destinationFolder"/> can be absolute or it may start with a special folder such as:
		///     %temp%, %localappdata%, etc...
		/// </remarks>
		/// <param name="sourceFilePath"></param>
		/// <param name="destinationFolder"></param>
		void CopyFile(string sourceFilePath, string destinationFolder);

		/// <summary>
		///     Copies the given files into the given folder.
		/// </summary>
		/// <param name="sourceFiles"></param>
		/// <param name="destinationFolder"></param>
		void CopyFiles(IEnumerable<string> sourceFiles, string destinationFolder);

		#endregion

		/// <summary>
		///     Copies the given installer to this client, executes it (using the given command-line) and finally removes the
		///     installer once more.
		/// </summary>
		/// <param name="installerPath"></param>
		/// <param name="commandLine"></param>
		void Install(string installerPath, string commandLine = null);

		void ExecuteFile(string clientFilePath, string commandLine = null);

		int ExecuteCommand(string cmd);

		#region Services

		/// <summary>
		/// Starts the given service, does nothing if the service is already running.
		/// </summary>
		/// <param name="serviceName"></param>
		void StartService(string serviceName);

		void StopService(string serviceName);

		#endregion
	}
}