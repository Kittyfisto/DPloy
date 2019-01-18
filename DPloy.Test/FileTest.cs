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
		[Test]
		public void TestUnequalHash()
		{
			var file = new Files();
			var hashA = file.CalculateSha256(@"TestData\1byte_a.txt");
			var hashB = file.CalculateSha256(@"TestData\1byte_b.txt");

			hashA.Should().NotEqual(hashB);
		}

		[Test]
		public void TestEqualHash()
		{
			var file = new Files();
			var hashA = file.CalculateSha256(@"TestData\1byte_a.txt");
			var hashB = file.CalculateSha256(@"TestData\1byte_a.txt");

			hashA.Should().Equal(hashB);
		}

		[Test]
		public void TestCalculateHashEmptyFile()
		{
			var file = new Files();
			var path = @"TestData\Empty.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestCalculateHash1byteFile()
		{
			var file = new Files();
			var path = @"TestData\1byte_a.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestCalculateHash4kFile()
		{
			var file = new Files();
			var path = @"TestData\4k.txt";
			var hash = file.CalculateSha256(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
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

		[Pure]
		private static byte[] CalculateSha256(string filePath)
		{
			using (var sha = SHA256.Create())
			using (var stream = File.OpenRead(filePath))
			{
				return sha.ComputeHash(stream);
			}
		}
	}
}