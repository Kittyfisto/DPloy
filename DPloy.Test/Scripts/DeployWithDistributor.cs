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
		public int Deploy(INode localNode, INode remoteNode)
		{
			if (localNode == null)
				return -1;

			if (remoteNode == null)
				return -2;

			return 0;
		}
	}
}
