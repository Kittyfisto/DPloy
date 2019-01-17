using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;

namespace DPloy.Core.SharpRemoteInterfaces
{
	[DataContract]
	public sealed class FileBatch
	{
		[DataMember(Name = "FilesToCopy")]
		public List<CopyFile> FilesToCopy;

		public FileBatch()
		{
			FilesToCopy = new List<CopyFile>();
		}

		[Pure]
		public bool Any()
		{
			// TODO: Include other instructions once they're here
			return FilesToCopy.Any();
		}
	}
}
