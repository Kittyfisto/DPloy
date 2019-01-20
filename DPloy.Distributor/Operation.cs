using System;

namespace DPloy.Distributor
{
	internal sealed class Operation
	{
		public const string Ok    = " [  OK  ] ";
		public const string Error = " [FAILED] ";

		private readonly string _message;
		private readonly int _maxLineLength;
		private readonly bool _verbose;

		public Operation(string message, int maxLineLength, bool verbose)
		{
			_message = message;
			_maxLineLength = maxLineLength;
			_verbose = verbose;
			WriteOperationBegin(message);
		}

		private static void WriteOperationBegin(string message)
		{
			Console.Write(message);
		}

		public void Success()
		{
			FillLine();
			WriteOperationSuccess();
		}

		public void Failed(Exception exception)
		{
			FillLine();
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

		private void FillLine()
		{
			if (_message.Length < _maxLineLength)
				Console.Write(new string(' ', _maxLineLength - _message.Length));
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