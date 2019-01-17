using System.IO;
using System.Reflection;
using CSScriptLibrary;
using DPloy.Core.PublicApi;
using log4net;

namespace DPloy.Distributor
{
	static class Executor
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void Run(string scriptFilePath)
		{
			var script = CompileScript(scriptFilePath);
			using (var distributor = new Distributor())
			{
				Log.InfoFormat("Executing '{0}'...", scriptFilePath);
				script.Run((IDistributor)distributor);
				Log.InfoFormat("Done!");
			}
		}

		private static dynamic CompileScript(string scriptFilePath)
		{
			Log.InfoFormat("Compiling '{0}'...", scriptFilePath);
			
			var script = File.ReadAllText(scriptFilePath);
			var evaluator = CSScript.Evaluator;
			return evaluator.LoadCode(script);
		}
	}
}