namespace DPloy
{
	class Application
	{
		public static void Run()
		{
			const string scriptFilePath = @"C:\Snapshots\DPloy\TestDeployment.cs";
			Executor.Run(scriptFilePath);
		}
	}
}
