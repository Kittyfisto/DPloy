using System;
using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public class ExecuteCmd
	{
		public void Deploy(INode node)
		{
			node.Execute(@"%System%\cmd.exe", timeout: TimeSpan.FromSeconds(1));
		}
	}
}
