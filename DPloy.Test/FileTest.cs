using System.Diagnostics.Contracts;
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
			var file = new File();
			var hashA = file.CalculateHash(@"TestData\1byte_a.txt");
			var hashB = file.CalculateHash(@"TestData\1byte_b.txt");

			hashA.Should().NotEqual(hashB);
		}

		[Test]
		public void TestEqualHash()
		{
			var file = new File();
			var hashA = file.CalculateHash(@"TestData\1byte_a.txt");
			var hashB = file.CalculateHash(@"TestData\1byte_a.txt");

			hashA.Should().Equal(hashB);
		}

		[Test]
		public void TestCalculateHashEmptyFile()
		{
			var file = new File();
			var path = @"TestData\Empty.txt";
			var hash = file.CalculateHash(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestCalculateHash1byteFile()
		{
			var file = new File();
			var path = @"TestData\1byte_a.txt";
			var hash = file.CalculateHash(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Test]
		public void TestCalculateHash4kFile()
		{
			var file = new File();
			var path = @"TestData\4k.txt";
			var hash = file.CalculateHash(path);
			var actualHash = CalculateSha256(path);

			hash.Should().Equal(actualHash);
		}

		[Pure]
		private static byte[] CalculateSha256(string filePath)
		{
			using (var sha = SHA256.Create())
			using (var stream = System.IO.File.OpenRead(filePath))
			{
				return sha.ComputeHash(stream);
			}
		}
	}
}