using System;
using System.Diagnostics.Contracts;
using System.IO;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Output;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class OperationTest
	{
		[Test]
		public void TestRemoteException()
		{
			var machineName = "That one PC in the basement";
			var exception = new RemoteNodeException(machineName,
			                                        new FileNotFoundException("Unable to find that goddamn file"));
			var message = WriteFailed(exception, verbose: false);
			message.Should().Contain($"Machine: {machineName}");
			message.Should().Contain("Unable to find that goddamn file");
		}

		[Test]
		public void TestTimeout()
		{
			var machineName = "That one PC in the basement";
			var exception = new RemoteNodeException(machineName,
			                                        new TimeoutException("The operation took too goddamn long"));
			var message = WriteFailed(exception, verbose: false);
			message.Should().Contain("TIMEDOUT");
			message.Should().Contain($"Machine: {machineName}");
			message.Should().Contain("The operation took too goddamn long");
		}

		[Pure]
		private static string WriteFailed(Exception e, bool verbose)
		{
			var writer = new StringWriter();
			var op = new ConsoleWriterOperation(writer, "", 120, verbose);
			op.Failed(e);
			return writer.ToString();
		}
	}
}
