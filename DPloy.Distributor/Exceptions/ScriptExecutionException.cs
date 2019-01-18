using System;

namespace DPloy.Distributor.Exceptions
{
	internal sealed class ScriptExecutionException : Exception
	{
		public ScriptExecutionException(string message)
			: base(message)
		{ }

		public ScriptExecutionException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}