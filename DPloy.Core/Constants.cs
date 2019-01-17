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

		public static readonly IApplicationConstants Node;
		public static readonly IApplicationConstants Distributor;

		static Constants()
		{
			FrameworkTitle = "DPloy";
			AppDataLocalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FrameworkTitle);

			var assembly = Assembly.GetExecutingAssembly();
			var name = assembly.GetName();
			FrameworkVersion = name.Version;

			Node = new ApplicationConstants(AppDataLocalFolder, "Node");
			Distributor = new ApplicationConstants(AppDataLocalFolder, "Distributor");
		}
	}
}