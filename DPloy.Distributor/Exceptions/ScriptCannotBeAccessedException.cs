using System;

namespace DPloy.Distributor.Exceptions
{
	internal sealed class ScriptCannotBeAccessedException
		: Exception
	{
		public ScriptCannotBeAccessedException(string message, Exception inner)
			: base(message, inner)
		{}
	}
}
