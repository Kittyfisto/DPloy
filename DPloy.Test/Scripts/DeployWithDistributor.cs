using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public class DeployWithDistributor
	{
		public int Deploy(IDistributor distributor, INode node)
		{
			if (distributor == null)
				return -1;

			if (node == null)
				return -2;

			return 0;
		}
	}
}
