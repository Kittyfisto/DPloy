using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPloy.Distributor.Output;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ConsoleWriterTest
	{
		[Test]
		public void TestExecute([Values(null, "")] string operationName)
		{
			var writer = new ConsoleWriter(false);
			var op = (ConsoleWriterOperation)writer.BeginExecute("calc.exe", "/?", operationName);
			op.Message.Should().Be("  Executing 'calc.exe /?'");
		}

		[Test]
		public void TestExecuteWithOperationName()
		{
			var writer = new ConsoleWriter(false);
			var op = (ConsoleWriterOperation)writer.BeginExecute("calc.exe", "/?", "Calculating the meaning of the universe and everything");
			op.Message.Should().Be("  Calculating the meaning of the universe and everything");
		}
	}
}
