using System;
using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public class ExecuteCalc
	{
		public void Deploy(INode node)
		{
			node.ExecuteFile(@"%System%\cmd.exe", timeout: TimeSpan.FromSeconds(1));
		}
	}
}
