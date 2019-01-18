using DPloy.Core.PublicApi;

public class Deployment
{
	public void Main(IDistributor distributor)
	{
		var node = distributor.ConnectTo("127.0.0.1", 48121);
		node.CopyFile(@"TestData\1byte_a.txt", @"%temp%\DPloy\Test\Scripts\");
	}
}
