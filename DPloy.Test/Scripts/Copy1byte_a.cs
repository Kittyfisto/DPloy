using DPloy.Core.PublicApi;

public class Deployment
{
	public void Deploy(INode node)
	{
		node.CopyFile(@"TestData\1byte_a.txt", @"%temp%\DPloy\Test\Scripts\1byte_a.txt");
	}
}
