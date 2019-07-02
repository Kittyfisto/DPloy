using System;
using System.Diagnostics;
using System.Threading;
using DPloy.Node.SharpRemoteImplementations;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ProcessesTest
	{
		[SetUp]
		public void Setup()
		{
			Thread.Sleep(TimeSpan.FromMilliseconds(10));
		}

		[Test]
		public void TestKillNonExistingProcess()
		{
			var processes = new Processes();
			new Action(() => processes.KillAll(new []{"I don't exist"})).Should().NotThrow("because killing non-running processes is not considered a failure");
		}

		[Test]
		public void TestKillOneProcess()
		{
			var process = Process.Start("calc.exe");
			process.HasExited.Should().BeFalse();

			var processes = new Processes();
			new Action(() => processes.KillAll(new[]{"calc"})).Should().NotThrow("because killing non-running processes is not considered a failure");

			process.HasExited.Should().BeTrue();
		}

		[Test]
		public void TestKillManyProcesses()
		{
			var process = Process.Start("calc.exe");
			process.HasExited.Should().BeFalse();

			var processes = new Processes();
			new Action(() => processes.KillAll(new[] { "npnawdowadwaaw", "calc" })).Should().NotThrow("because killing non-running processes is not considered a failure");

			process.HasExited.Should().BeTrue();
		}
	}
}