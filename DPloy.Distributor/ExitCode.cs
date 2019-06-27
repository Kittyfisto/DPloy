namespace DPloy.Distributor
{
	/// <summary>
	///     A list of possible error codes which may be returned by the distributor.
	/// </summary>
	public enum ExitCode
	{
		/// <summary>
		///     The program ran successfully.
		/// </summary>
		Success = 0,

		/// <summary>
		///     Invalid arguments were passed.
		/// </summary>
		InvalidArguments = 1,

		/// <summary>
		///     There was an error while trying to access the script.
		///     This can be due to any of the following:
		///     - The script file does not exist
		///     - The given path is too long
		///     - The script cannot be accessed because of insufficient rights
		/// </summary>
		ScriptCannotBeAccessed = 10,

		/// <summary>
		///     There was a problem compiling the script.
		///     Check the output for details.
		/// </summary>
		CompileError = 11,

		/// <summary>
		///     There was a problem while executing the script.
		///     This is most likely an exception which was not handled by the script.
		///     If you think that the exception is caused by a bug in this application, then please report it:
		///     https://github.com/Kittyfisto/DPloy/issues/new
		/// </summary>
		ExecutionError = 12,

		/// <summary>
		///     There was a problem establishing the connection to the node.
		/// </summary>
		ConnectionError = 100,

		/// <summary>
		///     There was an unhandled exception in this application.
		///     This is most certainly a bug and you'd be doing me a favour by reporting it:
		///     https://github.com/Kittyfisto/DPloy/issues/new
		/// </summary>
		UnhandledException = int.MaxValue
	}
}