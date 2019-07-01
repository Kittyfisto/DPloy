using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public class ExecuteDir
	{
		public void Deploy(INode node)
		{
			node.Execute(@"%system%\cmd.exe", "/c echo foo");
		}
	}
}
