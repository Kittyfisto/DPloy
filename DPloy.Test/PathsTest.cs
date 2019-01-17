using System;
using System.IO;
using DPloy.Core;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class PathsTest
	{
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
	}
}
