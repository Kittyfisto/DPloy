using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DPloy.Core.SharpRemoteInterfaces
{
	[DataContract]
	public struct ProcessOutput
	{
		[DataMember]
		public int ExitCode { get; set; }

		[DataMember]
		public string StandardOutput { get; set; }

		[DataMember]
		public string StandardError { get; set; }
	}
}
