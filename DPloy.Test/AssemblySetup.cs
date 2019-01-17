using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace DPloy.Test
{
	[SetUpFixture]
	public sealed class AssemblySetup
	{
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			var assemblyPath = Path.GetDirectoryName(path);
			var projectPath = Path.Combine(assemblyPath, "..", "..", "DPloy.Test");
			Directory.SetCurrentDirectory(projectPath);
		}
	}
}
