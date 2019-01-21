using DPloy.Distributor;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class NodeTest
	{
		[Test]
		public void TestGetPathRelativeTo()
		{
			NodeClient.GetPathRelativeTo(@"C:\windows\calc.exe", @"C:\windows\").Should().Be("calc.exe");
			NodeClient.GetPathRelativeTo(@"C:\windows\calc.exe", @"C:\windows").Should().Be("calc.exe");
			NodeClient.GetPathRelativeTo(@"C:/windows/calc.exe", @"C:/windows").Should().Be("calc.exe");
			NodeClient.GetPathRelativeTo(@"C:/windows/calc.exe", @"C:/windows/").Should().Be("calc.exe");
		}
	}
}