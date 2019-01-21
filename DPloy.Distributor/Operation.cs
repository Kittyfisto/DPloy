using System;
using System.IO;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor
{
	internal sealed class Operation
	{
		public const string Ok    = " [  OK  ] ";
		public const string Error = " [FAILED] ";

		private readonly TextWriter _writer;
		private readonly string _message;
		private readonly int _maxLineLength;
		private readonly bool _verbose;

		public Operation(TextWriter writer, string message, int maxLineLength, bool verbose)
		{
			_writer = writer;
			_message = message;
			_maxLineLength = maxLineLength;
			_verbose = verbose;
			WriteOperationBegin(message);
		}

		private void WriteOperationBegin(string message)
		{
			_writer.Write(message);
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
				_writer.Write(new string(' ', _maxLineLength - _message.Length));
		}

		private void WriteErrorMessage(AggregateException e)
		{
			foreach (dynamic innerException in e.InnerExceptions)
			{
				WriteErrorMessage(innerException);
			}
		}

		private void WriteErrorMessage(RemoteNodeException e)
		{
			_writer.WriteLine("\tMachine: {0}", e.MachineName);
			dynamic exception = e.InnerException;
			WriteErrorMessage(exception);
		}

		private void WriteErrorMessage(Exception e)
		{
			if (_verbose)
			{
				_writer.WriteLine("\t{0}", e);
			}
			else
			{
				_writer.WriteLine("\t{0}", e.Message);
			}
		}

		private void WriteLine(string message, ConsoleColor foregroundColor)
		{
			var previousForegroundColor = Console.ForegroundColor;
			try
			{
				Console.ForegroundColor = foregroundColor;
				_writer.WriteLine(message);
			}
			finally
			{
				Console.ForegroundColor = previousForegroundColor;
			}
		}
	}
}