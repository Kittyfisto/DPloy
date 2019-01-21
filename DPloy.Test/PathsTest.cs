using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DPloy.Core;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class PathsTest
	{
		//[Test]
		//public void TestSupportedFolders()
		//{
		//	var specialFolders = Enum.GetValues(typeof(Environment.SpecialFolder)).Cast<Environment.SpecialFolder>();
		//	var missingImplementations = new List<Environment.SpecialFolder>();
		//	foreach (var specialFolder in specialFolders)
		//	{
		//		try
		//		{
		//			Paths.NormalizeAndEvaluate($"%{specialFolder}%");
		//		}
		//		catch (Exception e)
		//		{
		//			missingImplementations.Add(specialFolder);
		//		}
		//	}

		//	missingImplementations.Should().BeEmpty();
		//}

		[Test]
		public void TestEvaluateTemp()
		{
			Paths.NormalizeAndEvaluate("%temp%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Path.GetTempPath(), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateDesktop()
		{
			Paths.NormalizeAndEvaluate("%Desktop%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateHistory()
		{
			Paths.NormalizeAndEvaluate("%history%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.History), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateAppData()
		{
			Paths.NormalizeAndEvaluate("%APPdata%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateLocalAppData()
		{
			Paths.NormalizeAndEvaluate("%localAppData%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateUserProfile()
		{
			Paths.NormalizeAndEvaluate("%UserProfile%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateWinDir()
		{
			Paths.NormalizeAndEvaluate("%WinDir%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateSystem()
		{
			Paths.NormalizeAndEvaluate("%System%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateSystemX86()
		{
			Paths.NormalizeAndEvaluate("%SystemX86%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateProgramFiles()
		{
			Paths.NormalizeAndEvaluate("%ProgramFiles%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateProgramFilesX86()
		{
			Paths.NormalizeAndEvaluate("%ProgramFilesx86%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateCommonProgramFiles()
		{
			Paths.NormalizeAndEvaluate("%CommonProgramFiles%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateCommonProgramFilesX86()
		{
			Paths.NormalizeAndEvaluate("%CommonProgramFilesX86%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluatePrograms()
		{
			Paths.NormalizeAndEvaluate("%Programs%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateMyDocuments()
		{
			Paths.NormalizeAndEvaluate("%MyDocuments%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateFavorites()
		{
			Paths.NormalizeAndEvaluate("%Favorites%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Favorites), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateStartup()
		{
			Paths.NormalizeAndEvaluate("%Startup%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateRecent()
		{
			Paths.NormalizeAndEvaluate("%Recent%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Recent), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateSendTo()
		{
			Paths.NormalizeAndEvaluate("%SendTo%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SendTo), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateStartMenu()
		{
			Paths.NormalizeAndEvaluate("%StartMenu%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateMyMusic()
		{
			Paths.NormalizeAndEvaluate("%MyMusic%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateMyVideos()
		{
			Paths.NormalizeAndEvaluate("%MyVideos%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "foo", "stuff"));
		}

		[Test]
		public void TestEvaluateDesktopDirectory()
		{
			Paths.NormalizeAndEvaluate("%DesktopDirectory%\\foo\\bar\\..\\stuff")
			     .Should().Be(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "foo", "stuff"));
		}
	}
}
