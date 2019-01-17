using DPloy.Core.PublicApi;

public class Deployment
{
	// This is the main entry point of a deployment script
	public void Run(IDistributor distributor)
	{
		//var node = distributor.ConnectTo("tsma6-101075");
		var node = distributor.ConnectTo("10.82.0.205", 49152);
		node.Install(@"C:\Users\miessler\Downloads\npp.7.6.2.Installer.exe");
		//node.Install(@"C:\Users\miessler\Downloads\setup-NESTOR-2.9.0-Build-14918-alpha_master.exe");
		//node.StartService("R&S RomesV TSMA");
		//node.StopService("R&S RomesV TSMA");
	}
}
