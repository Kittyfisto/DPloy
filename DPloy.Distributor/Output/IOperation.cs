using System;

namespace DPloy.Distributor.Output
{
	public interface IOperation
	{
		void Success();
		void Failed(Exception exception);
	}
}