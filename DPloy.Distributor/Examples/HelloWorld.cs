// This is an example deployment script (using cs-script)
using DPloy.Core.PublicApi;

public class Deployment
{
	/// <summary>
	///     The main entry point of this script.
	///     This method is called by Distributor.exe
	///     and is meant to contain the code to deploy software on remote machines.
	/// </summary>
	/// <param name="distributor">
	///     A reference to a distributor object which allows you to establish
	///     connections to remote machines where Node.exe is currently running
	/// </param>
	public static void Main(IDistributor distributor)
	{
		// An INode is an interface which allows you to copy files and execute commands
		// on a remote machine: In this case we're connecting a computer by its IPAddress
		// and port.
		using (INode node = distributor.ConnectTo("1.2.3.4", 1234))
		{
			// Copying files is done by specifying the source file path and the destination *directory*.
			node.CopyFile("HelloWorld.cs", @"%temp%\");
		}
	}
}
