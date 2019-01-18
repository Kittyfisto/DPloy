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
				Execute("Copy1byte_a.cs").Should().Be(0);
			}
		}

		[Test]
		public void TestForwardArguments([Values(1, 2, 3, int.MaxValue)] int argument)
		{
			Execute("ParseArgument.cs", new[]{argument.ToString()}).Should().Be(argument);
		}

		[Test]
		public void TestReturnValue()
		{
			Execute("Returns1337.cs").Should().Be(1337);
		}

		[Test]
		public void TestStaticMain()
		{
			Execute("StaticMainReturns9001.cs").Should().Be(9001);
		}

		[Test]
		public void TestPrivateMain()
		{
			Execute("PrivateMainReturns42.cs").Should().Be(42);
		}

		[Test]
		public void TestPrivateStaticMain()
		{
			Execute("PrivateStaticMainReturns101.cs").Should().Be(101);
		}

		private int Execute(string scriptFilePath, string[] args = null)
		{
			var fullScriptPath = Path.Combine(AssemblySetup.ScriptsDirectory, scriptFilePath);

			var arguments = new StringBuilder();
			arguments.AppendFormat("deploy --script \"{0}\"", fullScriptPath);

			if (args != null)
			{
				arguments.Append(" --scriptarguments");
				foreach (var argument in args)
				{
					arguments.Append(" ");
					arguments.Append(argument);
				}
			}

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
			return process.ExitCode;
		}
	}
}
