using System.IO;
using System.Net;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class DPloyAcceptanceTest
	{
		private static string GetTestTempDirectory()
		{
			var testClassName = TestContext.CurrentContext.Test.ClassName;
			var testMethodName = TestContext.CurrentContext.Test.MethodName;
			var destPath = Path.Combine(Path.GetTempPath(), "DPloy", "Test",
			                            testClassName,
			                            testMethodName);
			return destPath;
		}

		[SetUp]
		public void TestSetup()
		{
			var testDirectory = GetTestTempDirectory();
			if (Directory.Exists(testDirectory))
				Directory.Delete(testDirectory, true);
		}

		[Test]
		public void TestCopyFile1byte_a()
		{
			TestCopyFile("1byte_a.txt");
		}

		[Test]
		public void TestCopyFile4k()
		{
			TestCopyFile("4k.txt");
		}

		private void TestCopyFile(string fileName)
		{
			using (var node = new Node.Node())
			using (var deployer = new Core.Distributor())
			{
				var ep = node.Bind(IPAddress.Loopback);
				using (var client = deployer.ConnectTo(ep))
				{
					var sourceFilePath = Path.Combine("TestData", fileName);
					var destinationPath = GetTestTempDirectory();
					var destinationFilePath = Path.Combine(destinationPath, destinationPath, fileName);

					File.Exists(destinationFilePath).Should().BeFalse();
					client.CopyFile(sourceFilePath, destinationPath);

					File.Exists(destinationFilePath).Should().BeTrue();
					AreEqual(sourceFilePath, destinationFilePath).Should().BeTrue();
				}
			}
		}

		private bool AreEqual(string sourceFilePath, string destinationFilePath)
		{
			using (var source = File.OpenRead(sourceFilePath))
			using (var dest = File.OpenRead(destinationFilePath))
			{
				if (source.Length != dest.Length)
					return false;

				while (true)
				{
					if (source.Position >= source.Length)
						break;

					if (source.ReadByte() != dest.ReadByte())
						return false;
				}
			}

			return true;
		}
	}
}
