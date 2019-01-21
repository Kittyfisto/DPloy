using System;
using System.Diagnostics.Contracts;
using System.IO;
using DPloy.Distributor;
using DPloy.Distributor.Exceptions;
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

		[Pure]
		private static string WriteFailed(Exception e, bool verbose)
		{
			var writer = new StringWriter();
			var op = new Operation(writer, "", 120, verbose);
			op.Failed(e);
			return writer.ToString();
		}
	}
}
