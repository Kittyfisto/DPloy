using System.Runtime.Serialization;

namespace DPloy.Core.SharpRemoteInterfaces
{
	[DataContract]
	public sealed class CreateFile
	{
		[DataMember(Name = "FileName")]
		public string FilePath;

		[DataMember(Name = "Content")]
		public byte[] Content;
	}
}