using System;

namespace DPloy.Distributor.Exceptions
{
	/// <summary>
	///     An exception occured on a remote machine while calling a particular method on it.
	///     The original exception can be accessed using <see cref="Exception.InnerException"/>.
	/// </summary>
	internal sealed class RemoteNodeException
		: Exception
	{
		public readonly string MachineName;

		public RemoteNodeException(string machine, Exception innerException)
			: base($"An exception occured on {machine}", innerException)
		{
			MachineName = machine;
		}
	}
}