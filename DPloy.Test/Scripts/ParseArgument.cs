using DPloy.Core.PublicApi;

public class Deployment
{
	public int Main(IDistributor distributor, string[] args)
	{
		return int.Parse(args[0]);
	}
}
