using DPloy.Core.PublicApi;

namespace DPloy.Test.Scripts
{
	public sealed class DeployWithParameter
	{
		public int Deploy(INode node, string[] args)
		{
			if (args.Length == 2)
			{
				return int.Parse(args[0]) + int.Parse(args[1]);
			}

			return int.Parse(args[0]);
		}
	}
}
