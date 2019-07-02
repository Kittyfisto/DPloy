using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor.Output
{
	internal sealed class ConsoleWriterOperation
		: IOperation
	{
		public const string Ok      = " [   OK   ] ";
		public const string Error   = " [ FAILED ] ";
		public const string Timeout = " [TIMEDOUT] ";

		private readonly TextWriter _writer;
		private readonly string _message;
		private readonly int _maxLineLength;
		private readonly bool _verbose;

		public ConsoleWriterOperation(TextWriter writer, string message, int maxLineLength, bool verbose)
		{
			_writer = writer;
			_message = message;
			_maxLineLength = maxLineLength;
			_verbose = verbose;
			WriteOperationBegin(message);
		}

		public string Message => _message;

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
			WriteOperationFailed(exception);
			dynamic e = exception;
			WriteErrorMessage(e);
		}

		private void WriteOperationSuccess()
		{
			WriteLine(Ok, ConsoleColor.Green);
		}

		private void WriteOperationFailed(Exception exception)
		{
			WriteLine(IsTimeout(exception) ? Timeout : Error, ConsoleColor.Red);
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

		private static bool IsTimeout(Exception exception)
		{
			var exceptions = Flatten(exception);
			return exceptions.OfType<TimeoutException>().Any();
		}

		private static IReadOnlyList<Exception> Flatten(Exception exception)
		{
			var exceptions = new List<Exception>();
			while (exception != null)
			{
				exceptions.Add(exception);
				exception = exception.InnerException;
			}

			return exceptions;
		}
	}
}