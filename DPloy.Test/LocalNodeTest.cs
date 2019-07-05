using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DPloy.Distributor;
using DPloy.Distributor.Output;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class LocalNodeTest
	{
		private OperationTracker _operationTracker;
		private InMemoryFilesystem _fileSystem;
		private LocalNode _node;

		[SetUp]
		public void Setup()
		{
			_operationTracker = new OperationTracker();
			_fileSystem = new InMemoryFilesystem();
			_node = new LocalNode(_operationTracker, _fileSystem);

			_fileSystem.AddRoot(Path.GetPathRoot(Path.GetTempPath()));
			_fileSystem.CreateDirectory(Path.GetTempPath());
		}

		[Test]
		public void TestFileExists()
		{
			var fileName = "foo.txt";
			var filePath = Path.Combine("%temp%", fileName);
			var absoluteFilePath = Path.Combine(Path.GetTempPath(), fileName);
			_node.FileExists(filePath).Should().BeFalse();
			_node.FileExists(absoluteFilePath).Should().BeFalse();

			_fileSystem.DirectoryExists(Path.GetTempPath());
			_fileSystem.WriteAllText(Path.Combine(Path.GetTempPath(), fileName), "Whasup");
			_node.FileExists(filePath).Should().BeTrue();
			_node.FileExists(absoluteFilePath).Should().BeTrue();

			_fileSystem.DeleteFile(Path.Combine(Path.GetTempPath(), fileName));
			_node.FileExists(filePath).Should().BeFalse();
			_node.FileExists(absoluteFilePath).Should().BeFalse();
		}
	}
}
