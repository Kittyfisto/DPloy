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
		private readonly string _scriptPath;

		public ScriptCompilationException(string scriptPath, params string[] errors)
		{
			_scriptPath = scriptPath;
			_errors = errors;
		}

		public ScriptCompilationException(string scriptPath, CompilerException e)
			: base(e.Message, e)
		{
			_scriptPath = scriptPath;
			_errors = e.Data["Errors"] as List<string> ?? new List<string>();
			_warnings = e.Data["Warnings"] as List<string> ?? new List<string>();
		}

		public string ScriptPath
		{
			get { return _scriptPath; }
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
