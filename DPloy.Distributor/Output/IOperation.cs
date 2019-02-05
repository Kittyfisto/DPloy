using System;

namespace DPloy.Distributor.Output
{
	internal interface IOperation
	{
		void Success();
		void Failed(Exception exception);
	}
}