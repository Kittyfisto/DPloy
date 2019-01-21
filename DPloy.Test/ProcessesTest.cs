using System;
using System.Diagnostics;
using DPloy.Node.SharpRemoteImplementations;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ProcessesTest
	{
		[Test]
		public void TestKillNonExistingProcess()
		{
			var processes = new Processes();
			new Action(() => processes.KillAll("I don't exist")).Should().NotThrow("because killing non-running processes is not considered a failure");
		}

		[Test]
		public void TestKillOneProcess()
		{
			var process = Process.Start("calc.exe");
			process.HasExited.Should().BeFalse();

			var processes = new Processes();
			new Action(() => processes.KillAll("calc")).Should().NotThrow("because killing non-running processes is not considered a failure");

			process.HasExited.Should().BeTrue();
		}
	}
}