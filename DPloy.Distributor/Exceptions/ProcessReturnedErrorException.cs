using System;
using System.Diagnostics.Contracts;
using System.Text;
using DPloy.Core.SharpRemoteInterfaces;

namespace DPloy.Distributor.Exceptions
{
	internal sealed class ProcessReturnedErrorException : Exception
	{
		public ProcessReturnedErrorException(string imageFilePath, ProcessOutput output, bool printStdOut)
			: base(Format(imageFilePath, output, printStdOut))
		{}

		[Pure]
		private static string Format(string imageFilePath, ProcessOutput output, bool printStdOut)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("{0} returned {1}", imageFilePath, output.ExitCode);
			if (printStdOut)
			{
				builder.AppendLine();
				if (!string.IsNullOrEmpty(output.StandardOutput))
					builder.Append(output.StandardOutput);

				if (!string.IsNullOrEmpty(output.StandardError))
					builder.Append(output.StandardError);
			}

			return builder.ToString();
		}
	}
}
