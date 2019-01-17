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