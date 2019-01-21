using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using DPloy.Node.SharpRemoteImplementations;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class FileTest
	{
		#region Sha256

		[Test]
		public void TestSha256UnequalHash()
		{
			var file = new Files();
			var hashA = file.CalculateSha256(@"TestData\1byte_a.txt");
			var hashB = file.CalculateSha256(@"TestData\1byte_b.txt");

			hashA.Should().NotEqual(hashB);
		}

		[Test]
		public void TestSha256EqualHash()
		{
			var file = new Files();
			var hashA = file.CalculateSha256(@"TestData\1byte_a.txt");
			var hashB = file.CalculateSha256(@"TestData\1byte_a.txt");

			hashA.Should().Equal(hashB);
		}

		[Test]
		public void TestSha256CalculateHashEmptyFile()
		{
			var file = new Files();
			var path = @"TestData\Empty.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestSha256CalculateHash1byteFile()
		{
			var file = new Files();
			var path = @"TestData\1byte_a.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestSha256CalculateHash4kFile()
		{
			var file = new Files();
			var path = @"TestData\4k.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		#endregion
		
		#region MD5

		[Test]
		public void TestMD5UnequalHash()
		{
			var file = new Files();
			var hashA = file.CalculateMD5(@"TestData\1byte_a.txt");
			var hashB = file.CalculateMD5(@"TestData\1byte_b.txt");

			hashA.Should().NotEqual(hashB);
		}

		[Test]
		public void TestMD5EqualHash()
		{
			var file = new Files();
			var hashA = file.CalculateMD5(@"TestData\1byte_a.txt");
			var hashB = file.CalculateMD5(@"TestData\1byte_a.txt");

			hashA.Should().Equal(hashB);
		}

		[Test]
		public void TestMD5CalculateHashEmptyFile()
		{
			var file = new Files();
			var path = @"TestData\Empty.txt";
			var hash = file.CalculateMD5(path);
			var actualHash = CalculateMD5(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestMD5CalculateHash1byteFile()
		{
			var file = new Files();
			var path = @"TestData\1byte_a.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestMD5CalculateHash4kFile()
		{
			var file = new Files();
			var path = @"TestData\4k.txt";
			var hash = file.CalculateMD5(path);
			var actualHash = CalculateMD5(path);

			hash.Should().Equal(actualHash);
		}

		#endregion

		[Test]
		public void TestDeleteNonExistingFile()
		{
			var files = new Files();
			new Action(() => files.DeleteFileAsync("I don't exist.foo").Wait())
				.Should().NotThrow("because deleting a file which doesn't exist shouldn't be considered an error");
		}

		[Test]
		public void TestUnzip()
		{
			var testFolder = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "TestUnzip");
			var archivePath = Path.Combine(testFolder, "foo.zip");
			if (File.Exists(archivePath))
				File.Delete(archivePath);
			if (!Directory.Exists(testFolder))
				Directory.CreateDirectory(testFolder);

			using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				var entry = archive.CreateEntry("Test.txt");
				using (var stream = entry.Open())
				using (var writer = new StreamWriter(stream))
				{
					writer.Write("Hello, World!");
				}
			}

			var targetFolder = Path.Combine(testFolder, "Unzipped");
			if (Directory.Exists(targetFolder))
				Directory.Delete(targetFolder, recursive: true);

			var file = new Files();
			file.Unzip(archivePath, targetFolder, overwrite: false);

			var targetFile = Path.Combine(targetFolder, "Test.txt");
			File.Exists(targetFile).Should().BeTrue();
			File.ReadAllText(targetFile).Should().Be("Hello, World!");
		}

		[Test]
		public void TestUnzipNoOverwrite()
		{
			var testFolder = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "TestUnzip");
			var archivePath = Path.Combine(testFolder, "foo.zip");
			if (File.Exists(archivePath))
				File.Delete(archivePath);
			if (!Directory.Exists(testFolder))
				Directory.CreateDirectory(testFolder);

			using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				var entry = archive.CreateEntry("Test.txt");
				using (var stream = entry.Open())
				using (var writer = new StreamWriter(stream))
				{
					writer.Write("Hello, World!");
				}
			}

			var targetFolder = Path.Combine(testFolder, "Unzipped");
			if (!Directory.Exists(targetFolder))
				Directory.CreateDirectory(targetFolder);

			var targetFile = Path.Combine(targetFolder, "Test.txt");
			File.WriteAllText(targetFile, "Foobar");

			var file = new Files();
			new Action(() => file.Unzip(archivePath, targetFolder, overwrite: false))
				.Should().Throw<IOException>();

			File.Exists(targetFile).Should().BeTrue();
			File.ReadAllText(targetFile).Should().Be("Foobar");
		}

		[Test]
		public void TestUnzipOverwrite()
		{
			var testFolder = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "TestUnzip");
			var archivePath = Path.Combine(testFolder, "foo.zip");
			if (File.Exists(archivePath))
				File.Delete(archivePath);
			if (!Directory.Exists(testFolder))
				Directory.CreateDirectory(testFolder);

			using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
			{
				var entry = archive.CreateEntry("Test.txt");
				using (var stream = entry.Open())
				using (var writer = new StreamWriter(stream))
				{
					writer.Write("Hello, World!");
				}
			}

			var targetFolder = Path.Combine(testFolder, "Unzipped");
			if (!Directory.Exists(targetFolder))
				Directory.CreateDirectory(targetFolder);

			var targetFile = Path.Combine(targetFolder, "Test.txt");
			File.WriteAllText(targetFile, "Foobar");

			var file = new Files();
			file.Unzip(archivePath, targetFolder, overwrite: true);

			File.Exists(targetFile).Should().BeTrue();
			File.ReadAllText(targetFile).Should().Be("Hello, World!");
		}

		[Test]
		public void TestDeleteNonExistingDirectory()
		{
			var files = new Files();

			var path = Path.Combine(Path.GetTempPath(), "dwaadwdadasdawiodwahjwadjdawwad");
			new Action(() => files.DeleteDirectoryAsync(path, recursive: false).Wait())
				.Should().NotThrow();
		}

		[Test]
		public void TestDeleteEmptyDirectory()
		{
			var files = new Files();

			var path = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "EmptyDirectory");
			if (Directory.Exists(path))
				Directory.Delete(path, recursive: true);

			Directory.CreateDirectory(path);
			Directory.Exists(path).Should().BeTrue();

			files.DeleteDirectoryAsync(path, recursive: false).Wait();

			Directory.Exists(path).Should().BeFalse();
		}

		[Test]
		public void TestDeleteNonEmptyDirectory()
		{
			var files = new Files();

			var directory = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "NonEmptyDirectory");
			Directory.CreateDirectory(directory);
			Directory.Exists(directory).Should().BeTrue();

			var filePath = Path.Combine(directory, "SomeFile.txt");
			File.WriteAllText(filePath, "Hello!");
			File.Exists(filePath).Should().BeTrue();

			files.DeleteDirectoryAsync(directory, recursive: true).Wait();

			Directory.Exists(directory).Should().BeFalse();
			File.Exists(filePath).Should().BeFalse();
		}

		[Test]
		public void TestCreateDirectory()
		{
			var files = new Files();

			var directory = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "NonExistingDirectory");
			if (Directory.Exists(directory))
				Directory.Delete(directory);

			Directory.Exists(directory).Should().BeFalse();
			files.CreateDirectoryAsync(directory).Wait();

			Directory.Exists(directory).Should().BeTrue();
		}

		[Test]
		public void TestCreateExistingDirectory()
		{
			var files = new Files();

			var directory = Path.Combine(Path.GetTempPath(), "DPloy", "Test", "ExistingDirectory");
			Directory.CreateDirectory(directory);
			var file = Path.Combine(directory, "SomeFile.txt");
			File.WriteAllText(file, "Hello!");

			Directory.Exists(directory).Should().BeTrue();
			files.CreateDirectoryAsync(directory).Wait();

			Directory.Exists(directory).Should().BeTrue("because the directory should still exist");
			File.Exists(file).Should().BeTrue("Because the contents of the directory should not have been deleted");
			File.ReadAllText(file).Should().Be("Hello!", "Because the contents of the directory should not have been modified");
		}

		[Pure]
		private static byte[] CalculateSha256(string filePath)
		{
			using (var sha = SHA256.Create())
			using (var stream = File.OpenRead(filePath))
			{
				return sha.ComputeHash(stream);
			}
		}

		[Pure]
		private static byte[] CalculateMD5(string filePath)
		{
			using (var md5 = MD5.Create())
			using (var stream = File.OpenRead(filePath))
			{
				return md5.ComputeHash(stream);
			}
		}
	}
}