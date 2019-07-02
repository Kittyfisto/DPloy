using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DPloy.Core.Hash
{
	public static class HashCodeCalculator
	{
		[Pure]
		public static byte[] MD5(string filePath)
		{
			using (var algorithm = System.Security.Cryptography.MD5.Create())
				return CalculateHash(filePath, algorithm);
		}

		[Pure]
		private static byte[] CalculateHash(string filePath, HashAlgorithm algorithm)
		{
			using (var stream = File.OpenRead(filePath))
			{
				return CalculateHash(stream, algorithm);
			}
		}

		public static byte[] CalculateHash(Stream stream, HashAlgorithm algorithm)
		{
			const int bufferSize = 4096;
			var buffer = new Byte[bufferSize];

			while (true)
			{
				int bytesRead = stream.Read(buffer, 0, buffer.Length);
				if (bytesRead <= 0)
					break;

				algorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
			}

			algorithm.TransformFinalBlock(new byte[0], 0, 0);
			return algorithm.Hash;
		}

		[Pure]
		public static bool AreEqual(byte[] hash, byte[] otherHash)
		{
			if (hash.Length != otherHash.Length)
				return false;

			for (var i = 0; i < hash.Length; ++i)
				if (hash[i] != otherHash[i])
					return false;

			return true;
		}
		public static string ToString(byte[] hash)
		{
			StringBuilder hex = new StringBuilder(hash.Length * 2);
			foreach (byte b in hash)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}
	}
}
