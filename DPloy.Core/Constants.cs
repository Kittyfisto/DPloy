using System;
using System.IO;
using System.Reflection;

namespace DPloy.Core
{
	public static class Constants
	{
		public static readonly string FrameworkTitle;
		public static readonly Version FrameworkVersion;
		public static readonly string AppDataLocalFolder;
		
		#region Node

		public static readonly string NodeTitle;
		public static readonly string NodeAppDataLocalFolder;
		public static readonly string NodeLogFile;

		#endregion

		#region Distributor

		public static readonly string DistributorTitle;
		public static readonly string DistributorAppDataLocalFolder;
		public static readonly string DistributorLogFile;

		#endregion

		static Constants()
		{
			FrameworkTitle = "DPloy";
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FrameworkTitle);

			var assembly = Assembly.GetExecutingAssembly();
			var name = assembly.GetName();
			FrameworkVersion = name.Version;

			DistributorTitle = "Distributor";
			DistributorAppDataLocalFolder = Path.Combine(AppDataLocalFolder, DistributorTitle);
			DistributorLogFile = Path.Combine(DistributorAppDataLocalFolder, $"{DistributorTitle}.log");

			NodeTitle = "Node";
			NodeAppDataLocalFolder = Path.Combine(AppDataLocalFolder, NodeTitle);
			NodeLogFile = Path.Combine(NodeAppDataLocalFolder, $"{NodeTitle}.log");
		}
	}
}