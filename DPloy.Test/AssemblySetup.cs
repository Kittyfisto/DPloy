using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace DPloy.Test
{
	[SetUpFixture]
	public sealed class AssemblySetup
	{
		private static string _testProjectDirectory;
		private static string _assemblyDirectory;
		private static string _testDataDirectory;
		private static string _scriptsDirectory;

		public static string AssemblyDirectory => _assemblyDirectory;

		public static string TestProjectDirectory => _testProjectDirectory;

		public static string TestDataDirectory => _testDataDirectory;

		public static string ScriptsDirectory => _scriptsDirectory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			_assemblyDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

			_testProjectDirectory = Path.Combine(_assemblyDirectory, "..", "..", "DPloy.Test");

			Directory.SetCurrentDirectory(_testProjectDirectory);

			_testDataDirectory = Path.Combine(_testProjectDirectory, "TestData");
			_scriptsDirectory = Path.Combine(_testProjectDirectory, "Scripts");
		}
	}
}
