using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using DPloy.Node;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class DistributorAcceptanceTest
	{
		[Test]
		public void TestCopy1byte_a()
		{
			using (var node = new NodeServer())
			{
				node.Bind(new IPEndPoint(IPAddress.Loopback, 48121));

				Execute("Copy1byte_a.cs");
			}
		}

		private void Execute(string scriptFilePath)
		{
			var fullScriptPath = Path.Combine(AssemblySetup.ScriptsDirectory, scriptFilePath);

			var arguments = new StringBuilder();
			arguments.AppendFormat("--script \"{0}\"", fullScriptPath);

			var executablePath = Path.Combine(AssemblySetup.AssemblyDirectory, "Distributor.exe");
			var process = new Process
			{
				StartInfo = new ProcessStartInfo(executablePath)
				{
					Arguments = arguments.ToString(),
					WorkingDirectory = AssemblySetup.TestProjectDirectory,

					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				}
			};
			process.Start();

			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			TestContext.Progress.WriteLine(output);
			process.ExitCode.Should().Be(0);
		}
	}
}
