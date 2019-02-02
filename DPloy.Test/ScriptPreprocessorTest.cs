﻿using System;
using System.IO;
using System.Text;
using DPloy.Distributor;
using DPloy.Distributor.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class ScriptPreprocessorTest
	{
		private InMemoryFilesystem _fileSystem;

		[SetUp]
		public void Setup()
		{
			_fileSystem = new InMemoryFilesystem();
			_fileSystem.AddRoot(@"C:\");
		}

		[Test]
		public void TestPreprocessEmpty()
		{
			PreprocessScript("").Should().Be("");
		}

		[Test]
		public void TestPreprocessNoIncludes()
		{
			var script = "public static class Program { public static void Main() {} }";
			PreprocessScript(script).Should().Be(script);
		}

		[Test]
		public void TestPreprocessOneRelativeInclude()
		{
			var foo = "//css_import Bar\r\n"+
				"public static class Program { public static void Main() {} }";
			var fooPath = @"C:\foo.cs";
			WriteFile(fooPath, foo);

			var bar = "using namespace System;";
			var barPath = @"C:\bar.cs";
			WriteFile(barPath, bar);

			PreprocessFile(fooPath).Should().Be("using namespace System;\r\n" +
			                                    "public static class Program { public static void Main() {} }");
		}

		[Test]
		[Description("Verifies that a file may not include itself")]
		public void TestCyclicIncludesNotAllowed()
		{
			var foo = "//css_import foo\r\n"+
			          "public static class Program { public static void Main() {} }";
			var fooPath = @"C:\foo.cs";

			WriteFile(fooPath, foo);

			new Action(() => PreprocessFile(fooPath))
				.Should().Throw<AggregateException>()
				.WithInnerException<ScriptCompilationException>();
		}

		[Test]
		[Description("Verifies that files may not include previously included files")]
		public void TestDistantCyclicIncludesNotAllowed()
		{
			var foo = "//css_import bar";
			var fooPath = @"C:\foo.cs";
			WriteFile(fooPath, foo);

			var bar = "//css_import foo";
			var barPath = @"C:\bar.cs";
			WriteFile(barPath, bar);

			new Action(() => PreprocessFile(fooPath))
				.Should().Throw<AggregateException>()
				.WithInnerException<ScriptCompilationException>();
		}

		private string PreprocessScript(string script)
		{
			var filePath = @"C:\myscript.cs";
			WriteFile(filePath, script);
			return PreprocessFile(filePath);
		}

		private void WriteFile(string filePath, string script)
		{
			_fileSystem.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(script)).Wait();
		}

		private string PreprocessFile(string filePath)
		{
			var preprocessor = new ScriptPreprocessor(_fileSystem);
			var task = preprocessor.ProcessFileAsync(filePath, new string[0]);
			task.Wait(TimeSpan.FromSeconds(30)).Should().BeTrue("because parsing should have finished by now");
			return task.Result;
		}
	}
}