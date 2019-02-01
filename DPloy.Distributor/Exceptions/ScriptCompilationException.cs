using System;
using System.Collections.Generic;
using csscript;

namespace DPloy.Distributor.Exceptions
{
	internal sealed class ScriptCompilationException
		: Exception
	{
		private readonly IReadOnlyList<string> _warnings;
		private readonly IReadOnlyList<string> _errors;

		public ScriptCompilationException(params string[] errors)
		{
			_errors = errors;
		}

		public ScriptCompilationException(CompilerException e)
			: base(e.Message, e)
		{
			_errors = e.Data["Errors"] as List<string> ?? new List<string>();
			_warnings = e.Data["Warnings"] as List<string> ?? new List<string>();
		}

		public IReadOnlyList<string> Errors
		{
			get { return _errors; }
		}

		public IReadOnlyList<string> Warnings
		{
			get { return _warnings; }
		}
	}
}
