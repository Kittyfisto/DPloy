using DPloy.Distributor.Output;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ProgressWriterTest
	{
		[Test]
		public void TestPrunePath1()
		{
			const int maxLength = 45;
			var pruned =
				ConsoleWriter.PrunePath(@"C:\Users\miessler\Downloads\setup-NESTOR-2.9.0-Build-14918-alpha_master.exe",
					maxLength);
			pruned.Should().NotBeNull();
			pruned.Length.Should().Be(maxLength);
			pruned.Should().Be(@"C:\Users\miessler\Do[...]918-alpha_master.exe");
		}

		[Test]
		public void TestPrunePath2()
		{
			const int maxLength = 46;
			var pruned =
				ConsoleWriter.PrunePath(@"C:\Users\miessler\AppData\Local\Temp\DPloy\Test\DPloy.Test.NodeAcceptanceTest\TestCopyFile1byte_a\1byte_a.txt",
					maxLength);
			pruned.Should().NotBeNull();
			pruned.Length.Should().Be(maxLength);
			pruned.Should().Be(@"C:\Users\miessler\AppData\Loc[...]\1byte_a.txt");
		}
		
	}
}
