using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DPloy.Core;
using DPloy.Node;
using FluentAssertions;
using NUnit.Framework;
using ExitCode = DPloy.Distributor.ExitCode;

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
				node.Bind(new IPEndPoint(IPAddress.Loopback, Constants.ConnectionPort));

				Deploy("Copy1byte_a.cs", new []{IPAddress.Loopback.ToString()}, new string[0]).Should().Be(0);
			}
		}

		[Test]
		public void TestForwardArguments([Values(1, 2, 3, int.MaxValue)] int argument)
		{
			Run("ParseArgument.cs", new[]{argument.ToString()}).Should().Be(argument);
		}

		[Test]
		public void TestDeployForwardArguments([Values(1, 2, 3, int.MaxValue)] int argument)
		{
			using (var node = new NodeServer())
			{
				node.Bind(new IPEndPoint(IPAddress.Loopback, Constants.ConnectionPort));

				Deploy("DeployWithParameter.cs", new []{IPAddress.Loopback.ToString()}, new[]{argument.ToString()}).Should().Be(argument);
			}
		}

		[Test]
		public void TestDeployMultipleArguments()
		{
			using (var node = new NodeServer())
			{
				node.Bind(new IPEndPoint(IPAddress.Loopback, Constants.ConnectionPort));

				Deploy("DeployWithParameter.cs", new []{IPAddress.Loopback.ToString()}, new[]{"42", "9001"}).Should().Be(9043);
			}
		}

		[Test]
		public void TestReturnValue()
		{
			Run("Returns1337.cs").Should().Be(1337);
		}

		[Test]
		public void TestStaticMain()
		{
			Run("StaticMainReturns9001.cs").Should().Be(9001);
		}

		[Test]
		public void TestPrivateMain()
		{
			Run("PrivateMainReturns42.cs").Should().Be(42);
		}

		[Test]
		public void TestPrivateStaticMain()
		{
			Run("PrivateStaticMainReturns101.cs").Should().Be(101);
		}

		[Test]
		public void TestInvalidScriptPath()
		{
			Run("DoesNotExist.cs").Should().Be((int)Distributor.ExitCode.ScriptCannotBeAccessed);
		}

		[Test]
		public void TestScriptPathTooLong()
		{
			var path = "jiodawhoiwaohwadhohwahiahpihphipafwihafwawfppjodwmöwadmdwadwjhfwhioqfwhfwqdslkjlkajlkajldkajdkdjawadjwadlkjdawkldwjlkadwjlwdkajdwalkdwjaldwajlkwadjdwalkjdlwadjknmncmjfhuehhfldhfjhfjshooieoqweuoqwzeirjmpdjpfhqihrpqpqwojoqrjoei0fonfkmkrügkoüjkogrjwjworjwpwkwükwoküküwküw";
			Run(path).Should().Be((int)Distributor.ExitCode.ScriptCannotBeAccessed);
		}

		[Test]
		public void TestInvalidArguments()
		{
			Execute("dwadwawdaw").Should().Be((int)Distributor.ExitCode.InvalidArguments);
		}

		[Test]
		public void TestStartProcessWithTimeout()
		{
			using (var node = new NodeServer())
			{
				var ep = new IPEndPoint(IPAddress.Loopback, Constants.ConnectionPort);
				node.Bind(ep);

				var exitCode = ExitCode.Success;
				new Action(() =>
					{
						exitCode = (ExitCode) Deploy("ExecuteCmd.cs", new[] {ep.ToString()}, null);
					})
					.ExecutionTime().Should().BeLessOrEqualTo(TimeSpan.FromSeconds(10));
				exitCode.Should().Be(ExitCode.ExecutionError);
			}
		}

		[Test]
		public void TestStartProcessWithoutTimeout()
		{
			using (var node = new NodeServer())
			{
				var ep = new IPEndPoint(IPAddress.Loopback, Constants.ConnectionPort);
				node.Bind(ep);

				var exitCode = ExitCode.Success;
				new Action(() =>
					{
						exitCode = (ExitCode)Deploy("ExecuteDir.cs", new[] { ep.ToString() }, null);
					})
					.ExecutionTime().Should().BeLessOrEqualTo(TimeSpan.FromSeconds(10));
				exitCode.Should().Be(ExitCode.Success);
			}
		}

		private int Run(string scriptFilePath, string[] args = null)
		{
			var fullScriptPath = Path.Combine(AssemblySetup.ScriptsDirectory, scriptFilePath);

			var arguments = new StringBuilder();
			arguments.AppendFormat("run \"{0}\"", fullScriptPath);

			if (args != null)
			{
				foreach (var argument in args)
				{
					arguments.Append(" ");
					arguments.Append(argument);
				}
			}

			return Execute(arguments.ToString());
		}

		private int Deploy(string scriptFilePath, string[] nodes, string[] arguments)
		{
			var fullScriptPath = Path.Combine(AssemblySetup.ScriptsDirectory, scriptFilePath);

			var args = new StringBuilder();
			args.AppendFormat("deploy \"{0}\"", fullScriptPath);

			foreach (var node in nodes)
			{
				args.Append(" ");
				args.Append(node);
			}

			if (arguments != null && arguments.Any())
			{
				args.Append(" --arguments");
				foreach (var argument in arguments)
				{
					args.Append(" ");
					args.Append(argument);
				}
			}

			return Execute(args.ToString());
		}

		private static int Execute(string arguments)
		{
			TestContext.Progress.WriteLine("Arguments: {0}", arguments);

			var executablePath = Path.Combine(AssemblySetup.AssemblyDirectory, "Distributor.exe");
			var process = new Process
			{
				StartInfo = new ProcessStartInfo(executablePath)
				{
					Arguments = arguments,
					WorkingDirectory = AssemblySetup.TestProjectDirectory,

					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				}
			};

			var stopwatch = Stopwatch.StartNew();
			process.Start();

			var output = process.StandardOutput.ReadToEnd();
			var error = process.StandardError.ReadToEnd();
			process.WaitForExit();

			stopwatch.Stop();
			TestContext.Progress.WriteLine("{0} took {1}ms", executablePath, stopwatch.ElapsedMilliseconds);

			TestContext.Progress.WriteLine(!string.IsNullOrEmpty(output) ? output : error);


			return process.ExitCode;
		}
	}
}
