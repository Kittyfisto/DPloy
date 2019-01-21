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
		public static readonly string Desktop = "%Desktop%";
		public static readonly string History = "%History%";
		public static readonly string UserProfile = "%UserProfile%";
		public static readonly string WinDir = "%WinDir%";
		public static readonly string ProgramFiles = "%ProgramFiles%";
		public static readonly string ProgramFilesX86 = "%ProgramFilesX86%";
		public static readonly string System = "%System%";
		public static readonly string SystemX86 = "%SystemX86%";
		public static readonly string CommonProgramFiles = "%CommonProgramFiles%";
		public static readonly string CommonProgramFilesX86 = "%CommonProgramFilesX86%";
		public static readonly string Programs = "%Programs%";
		public static readonly string MyDocuments = "%MyDocuments%";
		public static readonly string Favorites = "%Favorites%";
		public static readonly string Startup = "%Startup%";
		public static readonly string Recent = "%Recent%";
		public static readonly string SendTo = "%SendTo%";
		public static readonly string StartMenu = "%StartMenu%";
		public static readonly string MyMusic = "%MyMusic%";
		public static readonly string MyVideos = "%MyVideos%";
		public static readonly string DesktopDirectory = "%DesktopDirectory%";

		static Paths()
		{
			SpecialFolders = new Dictionary<string, string>(new CaseInsensitveComparer())
			{
				{Temp, Path.GetTempPath()},
				{AppData, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)},
				{LocalAppData, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)},
				{Desktop, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))},
				{History, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.History))},
				{UserProfile, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))},
				{WinDir, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows))},
				{ProgramFiles, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles))},
				{ProgramFilesX86, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))},
				{System, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System))},
				{SystemX86, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86))},
				{CommonProgramFiles, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles))},
				{CommonProgramFilesX86, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86))},
				{Programs, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs))},
				{MyDocuments, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))},
				{Favorites, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Favorites))},
				{Startup, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup))},
				{Recent, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Recent))},
				{SendTo, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo))},
				{StartMenu, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu))},
				{MyMusic, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic))},
				{MyVideos, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos))},
				{DesktopDirectory, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory))},
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