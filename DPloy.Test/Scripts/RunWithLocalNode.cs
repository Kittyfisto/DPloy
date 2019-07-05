using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public class Deployment
	{
		int Run(INode node, string[] args)
		{
			if (node == null)
				return -1;

			return int.Parse(args[0]);
		}
	}
}
