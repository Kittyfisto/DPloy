using System;
using System.Security.Cryptography;

namespace DPloy.Core.Hash
{
	public sealed class HashCodeCalculator
		: IDisposable
	{
		private readonly SHA256 _sha;

		public HashCodeCalculator()
		{
			_sha = SHA256.Create();
		}

		public void Append(byte[] buffer, int length)
		{
			_sha.TransformBlock(buffer, 0, length, buffer, 0);
		}

		public byte[] CalculateHash()
		{
			_sha.TransformFinalBlock(new byte[0], 0, 0);
			return _sha.Hash;
		}

		#region IDisposable

		public void Dispose()
		{
			_sha?.Dispose();
		}

		#endregion
	}
}
