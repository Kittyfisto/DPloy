using DPloy.Core.PublicApi;

public class Deployment
{
	public int Run(string[] args)
	{
		return int.Parse(args[0]);
	}
}
