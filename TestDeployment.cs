using DPloy.Core.PublicApi;

public class Deployment
{
	// This is the main entry point of a deployment script
	public void Main(IDistributor distributor)
	{
		var node = distributor.ConnectTo("127.0.0.1");
		node.CopyFile("CommandLine.xml", @"X:\CommandLine.xml");
		//node.CopyFiles(new[] {"CommandLine.xml"}, "%temp%");
		//node.CopyDirectory(".", "%temp%");
	}
}
