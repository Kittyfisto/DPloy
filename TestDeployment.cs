using DPloy.Core.PublicApi;

public class Deployment
{
	// This is the main entry point of a deployment script
	public void Run(IDistributor distributor)
	{
		var node = distributor.ConnectTo("10.82.0.205", 49152);
		node.Install(@"C:\Users\miessler\Downloads\npp.7.6.2.Installer.exe");
	}
}
