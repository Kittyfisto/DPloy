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
				SetupLoggers();

				LogHeader(args);

				RegisterConsoleAppender();

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

		private static void SetupLoggers()
		{
			var hierarchy = (Hierarchy) LogManager.GetRepository();

			var patternLayout = new PatternLayout
			{
				ConversionPattern = "%date [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();

			var fileAppender = new RollingFileAppender
			{
				AppendToFile = false,
				File = Constants.DistributorLogFile,
				Layout = patternLayout,
				MaxSizeRollBackups = 3,
				MaximumFileSize = "1GB",
				RollingStyle = RollingFileAppender.RollingMode.Size,
				StaticLogFileName = false
			};
			fileAppender.ActivateOptions();
			hierarchy.Root.AddAppender(fileAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		private static void LogHeader(string[] args)
		{
			Log.InfoFormat("Starting {0} {1}...", Constants.FrameworkTitle, Constants.DistributorTitle);
			Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));
			LogEnvironment();
		}

		private static void LogEnvironment()
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendFormat("{0} {1}: v{2}, {3}\r\n",
			                     Constants.FrameworkTitle, Constants.DistributorTitle,
			                     FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
			                     Environment.Is64BitProcess ? "64bit" : "32bit");
			builder.AppendFormat(".NET Environment: {0}\r\n", Environment.Version);
			builder.AppendFormat("Operating System: {0}, {1}\r\n",
			                     Environment.OSVersion,
			                     Environment.Is64BitOperatingSystem ? "64bit" : "32bit");
			builder.AppendFormat("Current directory: {0}", Directory.GetCurrentDirectory());

			Log.InfoFormat("Environment: {0}", builder);
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