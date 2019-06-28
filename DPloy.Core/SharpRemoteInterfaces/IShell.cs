using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface IShell
	{
		[Invoke(Dispatch.SerializePerObject)]
		int ExecuteFile(string file, string commandLine);

		[Invoke(Dispatch.SerializePerObject)]
		int ExecuteCommand(string command);
	}
}