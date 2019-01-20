using System;

namespace DPloy.Distributor
{
	internal sealed class Operation
	{
		public const string Ok    = "[OK]    ";
		public const string Error = "[FAILED]";

		private readonly bool _verbose;

		public Operation(string message, bool verbose)
		{
			_verbose = verbose;
			WriteOperationBegin(message);
		}

		private static void WriteOperationBegin(string message)
		{
			Console.Write(message);
		}

		public void Success()
		{
			WriteOperationSuccess();
		}

		public void Failed(Exception exception)
		{
			WriteOperationFailed();
			dynamic e = exception;
			WriteErrorMessage(e);
		}

		private void WriteOperationSuccess()
		{
			WriteLine(Ok, ConsoleColor.Green);
		}

		private void WriteOperationFailed()
		{
			WriteLine(Error, ConsoleColor.Red);
		}

		private void WriteErrorMessage(AggregateException e)
		{
			foreach (dynamic innerException in e.InnerExceptions)
			{
				WriteErrorMessage(innerException);
			}
		}

		private void WriteErrorMessage(Exception e)
		{
			if (_verbose)
			{
				Console.WriteLine(e);
			}
			else
			{
				Console.WriteLine("\t{0}", e.Message);
			}
		}

		private static void WriteLine(string message, ConsoleColor foregroundColor)
		{
			var previousForegroundColor = Console.ForegroundColor;
			try
			{
				Console.ForegroundColor = foregroundColor;
				Console.WriteLine(message);
			}
			finally
			{
				Console.ForegroundColor = previousForegroundColor;
			}
		}
	}
}