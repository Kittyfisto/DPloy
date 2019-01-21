using System;
using DPloy.Node.SharpRemoteImplementations;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ServicesTest
	{
		[Test]
		[SetUICulture("en-US")]
		public void TestStartNonExistantService()
		{
			var services = new Services();
			new Action(() => services.Start("SomeNonExistingService"))
				.Should().Throw<ArgumentException>()
				.WithMessage("No such service: SomeNonExistingService");
		}

		[Test]
		[SetUICulture("en-US")]
		public void TestStopNonExistantService()
		{
			var services = new Services();
			new Action(() => services.Stop("SomeNonExistingService"))
				.Should().NotThrow();
		}
	}
}
