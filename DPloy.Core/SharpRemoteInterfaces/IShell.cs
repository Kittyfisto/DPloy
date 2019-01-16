using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface IShell
	{
		[Invoke(Dispatch.SerializePerObject)]
		int Execute(string command);
	}
}