namespace DPloy
{
	class Application
	{
		public static void Run()
		{
			const string scriptFilePath = @"C:\Users\Simon\Documents\GitHub\DPloy\TestDeployment.cs";
			Executor.Run(scriptFilePath);
		}
	}
}
