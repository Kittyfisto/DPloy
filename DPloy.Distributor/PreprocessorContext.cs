using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor
{
	/// <summary>
	///     Holds information relevant for preprocessing a script file before compilation.
	/// </summary>
	internal sealed class PreprocessorContext
	{
		private readonly HashSet<string> _includedScripts;
		private readonly List<string> _includeFolders;

		public PreprocessorContext(IReadOnlyList<string> includeFolders)
			: this(includeFolders, new HashSet<string>())
		{
		}

		public PreprocessorContext(IReadOnlyList<string> includeFolders,
		                           HashSet<string> includedScripts)
		{
			_includeFolders = includeFolders.ToList();
			_includedScripts = includedScripts;
		}

		public IEnumerable<string> IncludeFolders
		{
			get { return _includeFolders; }
		}

		[Pure]
		public PreprocessorContext CreateContextFor(string scriptFilePath)
		{
			if (!Path.IsPathRooted(scriptFilePath))
				throw new NotImplementedException($"Expected '{scriptFilePath}' to be an absolute file path!");

			if (_includedScripts.Contains(scriptFilePath))
				throw new ScriptCompilationException(scriptFilePath, "Cyclic includes are not allowed");

			var scriptFolder = Path.GetDirectoryName(scriptFilePath);
			var includeFolders = new List<string>(_includeFolders);
			if (!includeFolders.Contains(scriptFolder))
				includeFolders.Insert(index: 0, item: scriptFolder);

			var includedScripts = new HashSet<string>(_includedScripts);
			includedScripts.Add(scriptFilePath);
			return new PreprocessorContext(includeFolders, includedScripts);
		}

		[Pure]
		public static PreprocessorContext CreateFor(string scriptFilePath, IReadOnlyList<string> includeFolders)
		{
			var tmp = new PreprocessorContext(includeFolders);
			return tmp.CreateContextFor(scriptFilePath);
		}
	}
}