using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DPloy.Distributor.Exceptions;

namespace DPloy.Distributor
{
	/// <summary>
	///     Responsible for telling which sections of a preprocessed file belong to which file originally.
	///     Used to translate error message by the compiler so they contain ACTUAL the line numbers and file names, not
	///     the "fake" file created due to preprocessing.
	/// </summary>
	internal sealed class PreprocessTable
	{
		
	}

	/// <summary>
	///     Responsible for processing a script file before it's sent off to the compiler.
	/// </summary>
	internal sealed class ScriptPreprocessor
	{
		private static readonly Regex ImportRegexp;
		private readonly IFilesystem _fileSystem;

		static ScriptPreprocessor()
		{
			ImportRegexp = new Regex(@"//\s*css_import (.[^\r\n]*)", RegexOptions.Compiled | RegexOptions.Singleline);
		}

		public ScriptPreprocessor(IFilesystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		/// <summary>
		/// </summary>
		/// <param name="scriptFilePath"></param>
		/// <param name="includeFolders"></param>
		/// <returns>The preprocessed content of the script at the given path</returns>
		public string ProcessFile(string scriptFilePath, IReadOnlyList<string> includeFolders)
		{
			var content = PreProcessFile(MakeAbsolute(scriptFilePath),
			                             PreprocessorContext.CreateFor(scriptFilePath, includeFolders));
			return Process(content);
		}

		private string Process(string content)
		{
			var namespaces = RemoveUsings(content, out var tmp);

			int i = 0;
			foreach (var @namespace in namespaces)
			{
				var snippet = $"using namespace {@namespace};\r\n";
				tmp.Insert(i, snippet);
				i += snippet.Length;
			}

			return tmp.ToString();
		}

		private static HashSet<string> RemoveUsings(string content, out StringBuilder builder)
		{
			var namespaces = new HashSet<string>();
			builder = new StringBuilder(content.Length);
			var regex = new Regex(@"\s*using\s+namespace\s+([^;\s]+)\s*;");
			int i;
			for(i = 0; i < content.Length;)
			{
				var match = regex.Match(content, i, content.Length - i);
				if (!match.Success)
					break;

				var start = match.Index;
				var end = start + match.Length;
				builder.Append(content, i, start - i);
				var @namespace = match.Groups[1].Value.Trim();
				namespaces.Add(@namespace);

				i = end;
			}

			builder.Append(content, i, content.Length - i);

			return namespaces;
		}

		private string PreProcessFile(string scriptFilePath, PreprocessorContext context)
		{
			try
			{
				var scriptContent = _fileSystem.ReadAllText(scriptFilePath);
				var processedScript = new StringBuilder();
				var lastIndex = 0;
				foreach (Match match in ImportRegexp.Matches(scriptContent))
				{
					processedScript.Append(scriptContent, lastIndex, match.Index - lastIndex);

					var includedFilePath = match.Groups[groupnum: 1].Value.Trim();
					var subScriptFilePath = ResolveScriptPath(includedFilePath, context);
					var subScriptContent =
						PreProcessFile(subScriptFilePath, context.CreateContextFor(subScriptFilePath));

					processedScript.Append(subScriptContent);

					lastIndex = match.Index + match.Length;
				}

				if (lastIndex < scriptContent.Length)
					processedScript.Append(scriptContent, lastIndex, scriptContent.Length - lastIndex);

				return processedScript.ToString();
			}
			catch (IOException e)
			{
				throw new ScriptCannotBeAccessedException(e.Message, e);
			}
		}

		[Pure]
		private string MakeAbsolute(string scriptFilePath)
		{
			if (Path.IsPathRooted(scriptFilePath))
				return scriptFilePath;

			return Path.Combine(_fileSystem.CurrentDirectory, scriptFilePath);
		}

		private string ResolveScriptPath(string filePath, PreprocessorContext context)
		{
			if (Path.IsPathRooted(filePath))
				return filePath;

			foreach (var includeFolder in context.IncludeFolders)
			{
				var path = Path.Combine(includeFolder, filePath + ".cs");
				if (_fileSystem.FileExists(path))
					return path;
			}

			throw new NotImplementedException();
		}
	}
}