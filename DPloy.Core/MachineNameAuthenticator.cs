using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;
using DPloy.Core.Hash;
using SharpRemote;

namespace DPloy.Core
{
	/// <summary>
	///     Responsible for authenticating incoming connections: It will only allow
	///     connections from certain machines.
	/// </summary>
	public sealed class MachineNameAuthenticator
		: IAuthenticator
	{
		private readonly string _machineName;
		private readonly string[] _allowedNames;

		private MachineNameAuthenticator(string[] allowedMachineNames)
		{
			_allowedNames = allowedMachineNames;
		}

		private MachineNameAuthenticator()
		{
			_machineName = Environment.MachineName;
		}

		[Pure]
		public static MachineNameAuthenticator CreateForServer(string[] allowedMachineNames)
		{
			return new MachineNameAuthenticator(allowedMachineNames ?? new string[0]);
		}

		[Pure]
		public static MachineNameAuthenticator CreateClient()
		{
			return new MachineNameAuthenticator();
		}

		public string CreateChallenge()
		{
			return Guid.NewGuid().ToString();
		}

		public string CreateResponse(string challenge)
		{
			return CreateResponse(challenge, _machineName);
		}

		public bool Authenticate(string challenge, string response)
		{
			foreach (var allowedName in _allowedNames)
			{
				var expectedResponse = CreateResponse(challenge, allowedName);
				if (string.Equals(expectedResponse, response))
					return true;
			}

			return false;
		}

		private static string CreateResponse(string challenge, string machineName)
		{
			const int numIterations = 999;
			const int length = 16;
			var k = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(machineName),
				Encoding.UTF8.GetBytes(challenge), numIterations);
			return HashCodeCalculator.ToString(k.GetBytes(length));
		}
	}
}