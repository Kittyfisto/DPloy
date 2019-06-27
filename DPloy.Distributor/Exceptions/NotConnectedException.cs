using System;

namespace DPloy.Distributor.Exceptions
{
	internal sealed class NotConnectedException
		: Exception
	{
		public NotConnectedException(Exception inner)
			: base(inner.Message, inner)
		{}
	}
}
