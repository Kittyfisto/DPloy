using System.Runtime.Serialization;

namespace DPloy.Core.SharpRemoteInterfaces
{
	[DataContract]
	public sealed class CopyFile
	{
		[DataMember(Name = "FileName")]
		public string FileName;

		[DataMember(Name = "Content")]
		public byte[] Content;
	}
}