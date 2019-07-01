using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPloy.Core.PublicApi
{
	/// <summary>
	///     The public interface of the distributor - allows to perform operations on the computer the script is running.
	/// </summary>
	/// <remarks>
	///     All methods offered by this interface block until either they succeed or an unrecoverable error occured
	///     (such as file not existing, or not writable, etc...)
	/// </remarks>
	/// <remarks>
	///     Parameters such as 'sourceFilePath' refer to a path on the system where the deployment script is executed.
	///     Parameters such as 'destinationFilePath' refer to a path on the system where the deployment script is executed.
	/// </remarks>
	public interface IDistributor
	{
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
		/// <exception cref="Exception">In case the process exits with a non-zero value</exception>
		void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null);
	}
}
