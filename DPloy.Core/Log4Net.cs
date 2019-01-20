using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace DPloy.Core
{
	public static class Log4Net
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Initializes log4net to log to both a file as well as to the console.
		/// </summary>
		/// <param name="constants"></param>
		/// <param name="args"></param>
		/// <param name="logToConsole"></param>
		public static void Setup(IApplicationConstants constants, string[] args, bool logToConsole = false)
		{
			RegisterFileAppender(constants);
			LogHeader(Constants.Distributor, args);
			if (logToConsole)
				RegisterConsoleAppender();
		}

		private static void RegisterFileAppender(IApplicationConstants constants)
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
				File = constants.LogFile,
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

		private static void LogHeader(IApplicationConstants constants, string[] args)
		{
			Log.InfoFormat("Starting {0} {1}...", Constants.FrameworkTitle, constants.Title);
			Log.InfoFormat("Commandline arguments: {0}", string.Join(" ", args));
			LogEnvironment(constants);
		}

		private static void LogEnvironment(IApplicationConstants constants)
		{
			var builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendFormat("{0} {1}: v{2}, {3}\r\n",
								 Constants.FrameworkTitle, constants.Title,
								 FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion,
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
			var hierarchy = (Hierarchy)LogManager.GetRepository();

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
