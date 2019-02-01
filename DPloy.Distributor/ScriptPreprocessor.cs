using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
		public async Task<string> ProcessFileAsync(string scriptFilePath, IReadOnlyList<string> includeFolders)
		{
			return await ProcessFileAsync(MakeAbsolute(scriptFilePath),
			                              PreprocessorContext.CreateFor(scriptFilePath, includeFolders));
		}

		private async Task<string> ProcessFileAsync(string scriptFilePath, PreprocessorContext context)
		{
			try
			{
				using (var stream = await _fileSystem.OpenRead(scriptFilePath))
				using (var reader = new StreamReader(stream))
				{
					var scriptContent = await reader.ReadToEndAsync();

					var processedScript = new StringBuilder();
					var lastIndex = 0;
					foreach (Match match in ImportRegexp.Matches(scriptContent))
					{
						processedScript.Append(scriptContent, lastIndex, match.Index - lastIndex);

						var includedFilePath = match.Groups[groupnum: 1].Value.Trim();
						var subScriptFilePath = await ResolveScriptPathAsync(includedFilePath, context);
						var subScriptContent =
							await ProcessFileAsync(subScriptFilePath, context.CreateContextFor(subScriptFilePath));

						processedScript.Append(subScriptContent);

						lastIndex = match.Index + match.Length;
					}

					if (lastIndex < scriptContent.Length)
						processedScript.Append(scriptContent, lastIndex, scriptContent.Length - lastIndex);

					return processedScript.ToString();
				}
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

		private async Task<string> ResolveScriptPathAsync(string filePath, PreprocessorContext context)
		{
			if (Path.IsPathRooted(filePath))
				return filePath;

			foreach (var includeFolder in context.IncludeFolders)
			{
				var path = Path.Combine(includeFolder, filePath + ".cs");
				if (await _fileSystem.FileExists(path))
					return path;
			}

			throw new NotImplementedException();
		}
	}
}