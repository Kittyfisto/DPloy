using DPloy.Core.PublicApi;

public class Deployment
{
	// This is the main entry point of a deployment script
	public void Main(IDistributor distributor)
	{
		var node = distributor.ConnectTo("127.0.0.1", 49152);
		node.CopyFile("CommandLine.xml", @"%temp%\CommandLine.xml");
		node.CopyFiles(new[] {"CommandLine.xml"}, "%temp%");
		//node.CopyDirectory(".", "%temp%");
		node.CopyAndUnzipArchive(@"C:\Users\Simon\Documents\GitHub\DPloy\foo.zip", @"%temp%\zip");
	}
}
