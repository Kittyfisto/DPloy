using System;
using System.IO;
using System.Linq;
using System.Net;
using DPloy.Core;
using DPloy.Core.PublicApi;
using DPloy.Distributor.Output;
using DPloy.Node;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class NodeAcceptanceTest
	{
		private NodeServer _nodeServer;
		private Distributor.Distributor _distributor;
		private INode _nodeClient;

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

			_nodeServer = new NodeServer();
			_distributor = new Distributor.Distributor(new ConsoleWriter(true));
			var ep = _nodeServer.Bind(IPAddress.Loopback);
			_nodeClient = _distributor.ConnectTo(ep);
		}

		[TearDown]
		public void TearDown()
		{
			_nodeClient?.Dispose();
			_distributor?.Dispose();
			_nodeServer?.Dispose();
		}

		[Test]
		public void TestExecuteCommand()
		{
			_nodeClient.ExecuteCommand("EXIT 101").Should().Be(101);
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

		[Test]
		public void TestDownloadFile()
		{
			var filePath = Path.Combine(GetTestTempDirectory(), "www.google.de", "index.html");
			File.Exists(filePath).Should().BeFalse();

			_nodeClient.DownloadFile("http://www.google.de/index.html", filePath);

			File.Exists(filePath).Should().BeTrue();
			new FileInfo(filePath).Length.Should().BeGreaterThan(0);
		}

		[Test]
		public void TestCopySeveralFiles()
		{
			var sourceFileNames = new[] {"1byte_a.txt", "1byte_b.txt", "4k.txt", "Empty.txt"};
			var sourceFiles = sourceFileNames.Select(x => Path.Combine("TestData", x));
			var destinationFolder = GetTestTempDirectory();
			var destinationFiles = sourceFileNames.Select(x => Path.Combine(destinationFolder, x));

			foreach (var file in destinationFiles)
				File.Exists(file).Should().BeFalse();

			_nodeClient.CopyFiles(sourceFiles, destinationFolder);

			foreach (var file in destinationFiles)
				File.Exists(file).Should().BeTrue();
		}

		[Test]
		public void TestCopyDirectoryNonExistingSource()
		{
			var destinationPath = @"%temp%\foo\bar";
			new Action(() => _nodeClient.CopyDirectory(@"%temp%\some non existing dir\dawdaw\", destinationPath))
				.Should().Throw<DirectoryNotFoundException>();

			Directory.Exists(Paths.NormalizeAndEvaluate(destinationPath)).Should().BeFalse();
		}

		[Test]
		public void TestCopyEmptyDirectory()
		{
			var sourcePath = Path.Combine(GetTestTempDirectory(), "source");
			var destPath = Path.Combine(GetTestTempDirectory(), "dest");

			Directory.CreateDirectory(sourcePath);
			Directory.Exists(destPath).Should().BeFalse();

			_nodeClient.CopyDirectory(sourcePath, destPath);

			Directory.Exists(destPath).Should().BeTrue();
		}

		[Test]
		public void TestCreateNonExistingFile()
		{
			var filePath = Path.Combine(GetTestTempDirectory(), "A file.bin");
			File.Exists(filePath).Should().BeFalse();

			var content = new byte[] {1, 2, 3, 4};
			_nodeClient.CreateFile(filePath, content);

			File.Exists(filePath).Should().BeTrue();
			File.ReadAllBytes(filePath).Should().Equal(content);
		}

		[Test]
		public void TestCreateExistingFile()
		{
			var filePath = Path.Combine(GetTestTempDirectory(), "A file.bin");
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			File.WriteAllBytes(filePath, new byte[] {1, 2, 3, 4, 5});

			var content = new byte[] {1, 2, 3, 4};
			_nodeClient.CreateFile(filePath, content);

			File.Exists(filePath).Should().BeTrue();
			File.ReadAllBytes(filePath).Should().Equal(content);
		}

		[Test]
		public void TestDeleteNonExistingFile()
		{
			var file = Path.Combine(GetTestTempDirectory(), "Idontexist.bin");
			new Action(() => _nodeClient.DeleteFile(file)).Should().NotThrow();
		}

		[Test]
		public void TestDeleteExistingFile()
		{
			var filePath = Path.Combine(GetTestTempDirectory(), "Hello.txt");
			WriteAllText(filePath, "I'm there!");
			File.Exists(filePath).Should().BeTrue();

			_nodeClient.DeleteFile(filePath);

			File.Exists(filePath).Should().BeFalse();
		}

		[Test]
		public void TestNonExistingDirectory()
		{
			var directory = Path.Combine(GetTestTempDirectory(), "Idontexist");
			new Action(() => _nodeClient.DeleteDirectoryRecursive(directory)).Should().NotThrow();
		}

		[Test]
		public void TestCopyDirectoryRecursive()
		{
			var sourceDirectory = Path.Combine(GetTestTempDirectory(), "source");
			var filePath1 = Path.Combine(sourceDirectory, "A.txt");
			var fileContent1 = "Stuff";
			WriteAllText(filePath1, fileContent1);
			var filePath2 = Path.Combine(sourceDirectory, "B", "B.txt");
			var fileContent2 = "Hello, World!";
			WriteAllText(filePath2, fileContent2);

			var destinationDirectory = Path.Combine(GetTestTempDirectory(), "destination");
			_nodeClient.CopyDirectoryRecursive(sourceDirectory, destinationDirectory);

			var destFilePath1 = Path.Combine(destinationDirectory, "A.txt");
			File.Exists(destFilePath1).Should().BeTrue();

			var destFilePath2 = Path.Combine(destinationDirectory, "B", "B.txt");
			File.Exists(destFilePath2).Should().BeTrue();
		}

		private void WriteAllText(string filePath, string fileContent1)
		{
			var directory = Path.GetDirectoryName(filePath);
			Directory.CreateDirectory(directory);
			File.WriteAllText(filePath, fileContent1);
		}

		private void TestCopyFile(string fileName)
		{
			var sourceFilePath = Path.Combine("TestData", fileName);
			var destinationFilePath = Path.Combine(GetTestTempDirectory(), fileName);

			File.Exists(destinationFilePath).Should().BeFalse();
			_nodeClient.CopyFile(sourceFilePath, destinationFilePath);

			File.Exists(destinationFilePath).Should().BeTrue();
			AreEqual(sourceFilePath, destinationFilePath).Should().BeTrue();
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
