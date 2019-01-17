using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace DPloy.Core
{
	public static class Paths
	{
		private static readonly Dictionary<string, string> SpecialFolders;

		public static readonly string Temp = "%Temp%";
		public static readonly string AppData = "%AppData%";
		public static readonly string LocalAppData = "%LocalAppData%";

		static Paths()
		{
			SpecialFolders = new Dictionary<string, string>(new CaseInsensitveComparer())
			{
				{Temp, Path.GetTempPath()},
				{AppData, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)},
				{LocalAppData, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}
			};
		}

		[Pure]
		public static string NormalizeAndEvaluate(string relativeOrAbsolutePath)
		{
			if (relativeOrAbsolutePath.StartsWith("%"))
				relativeOrAbsolutePath = Evaluate(relativeOrAbsolutePath);

			return Normalize(relativeOrAbsolutePath);
		}

		/// <summary>
		///     Normalizes the given path: If the path is relative, it is made absolute using the current
		///     working directory.
		/// </summary>
		/// <param name="relativeOrAbsolutePath"></param>
		/// <returns></returns>
		private static string Normalize(string relativeOrAbsolutePath)
		{
			if (!Path.IsPathRooted(relativeOrAbsolutePath))
				relativeOrAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), relativeOrAbsolutePath);

			return Path.GetFullPath(new Uri(relativeOrAbsolutePath).LocalPath);
		}

		/// <summary>
		///     Expands the given path so that fragments such as %temp% point to the actual temp directory,
		///     for example C:\users\simon\AppData\Local\Temp
		/// </summary>
		/// <param name="relativeOrAbsolutePath"></param>
		/// <returns></returns>
		[Pure]
		public static string Evaluate(string relativeOrAbsolutePath)
		{
			var endIndex = relativeOrAbsolutePath.IndexOf("%", startIndex: 1);
			if (endIndex == -1)
				throw new ArgumentException($"Expected a closing % in path: {relativeOrAbsolutePath}");

			var specialFolder = relativeOrAbsolutePath.Substring(0, length: endIndex + 1);
			if (!SpecialFolders.TryGetValue(specialFolder, out var expandedPath))
				throw new
					ArgumentException($"Unknown special folder '{specialFolder}' in path: {relativeOrAbsolutePath}");

			var builder = new StringBuilder(relativeOrAbsolutePath);
			builder.Remove(startIndex: 0, length: specialFolder.Length);
			builder.Insert(index: 0, value: expandedPath);
			return builder.ToString();
		}

		private sealed class CaseInsensitveComparer
			: IEqualityComparer<string>
		{
			#region Implementation of IEqualityComparer<in string>

			public bool Equals(string x, string y)
			{
				return string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);
			}

			public int GetHashCode(string obj)
			{
				return obj.ToLowerInvariant().GetHashCode();
			}

			#endregion
		}
	}
}