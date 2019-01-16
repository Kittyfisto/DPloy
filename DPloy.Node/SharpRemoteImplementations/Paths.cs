using System.IO;
using DPloy.Core.SharpRemoteInterfaces;

namespace DPloy.Node.SharpRemoteImplementations
{
	sealed class Paths
		: IPaths
	{
		#region Implementation of IPaths

		public string GetTempPath()
		{
			return Path.GetTempPath();
		}

		#endregion
	}
}
