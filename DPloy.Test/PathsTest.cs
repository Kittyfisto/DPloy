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
	}
}
