using System;

namespace DPloy.Distributor.Output
{
	internal sealed class Operation
		: IOperation
	{
		private Exception _exception;

		#region Implementation of IOperation

		public void Success()
		{}

		public void Failed(Exception exception)
		{
			_exception = exception;
		}

		public void ThrowOnFailure()
		{
			if (_exception != null)
				throw _exception;
		}

		#endregion
	}
}