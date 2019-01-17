using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using DPloy.Core;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace DPloy
{
	static class Bootstrapper
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		static int Main(string[] args)
		{
			try
			{
				Log4Net.Setup(Constants.Distributor, args);

				Application.Run();
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("Terminating due to unexpected exception:");
				Console.WriteLine(e);
				return -1;
			}
		}

		private static void RegisterConsoleAppender()
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date %-5level - %message%newline"
			};
			patternLayout.ActivateOptions();

			var consoleAppender = new ConsoleAppender
			{
				Layout = patternLayout
			};
			consoleAppender.ActivateOptions();
			hierarchy.Root.AddAppender(consoleAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}
	}
}