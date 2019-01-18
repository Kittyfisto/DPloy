using System;

namespace DPloy.Distributor
{
	internal sealed class ScriptExecutionException : Exception
	{
		public ScriptExecutionException(string message)
			: base(message)
		{ }
	}
}