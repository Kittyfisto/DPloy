using System;
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
			                                    "\r\n" +
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
				.Should().Throw<ScriptCompilationException>();
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
				.Should().Throw<ScriptCompilationException>();
		}

		[Test]
		[Description("")]
		public void TestFixUsingStatements()
		{
			var main = "//css_import Foo\r\n"+
			          "public static class Program { public static void Main() {} }";
			var mainPath = @"C:\main.cs";
			WriteFile(mainPath, main);

			var foo = "//css_import Bar\r\n" +
				" using namespace System;\r\n"+
				"public static class Foo { public static int Get() { return 42; } }";
			var fooPath = @"C:\foo.cs";
			WriteFile(fooPath, foo);

			var bar = "  using  namespace System\t\t ;\r\n"+
					  "\t\tusing \t namespace System.IO ;\r\n" +
				"public static class Bar { public static string Get() { return \"Whatever...\"; } }";
			var barPath = @"C:\bar.cs";
			WriteFile(barPath, bar);

			PreprocessFile(mainPath).Should().Be("using namespace System;\r\n" +
			                                     "using namespace System.IO;\r\n" +
			                                     "\r\n" +
			                                     "public static class Bar { public static string Get() { return \"Whatever...\"; } }\r\n" +
			                                     "public static class Foo { public static int Get() { return 42; } }\r\n" +
			                                     "public static class Program { public static void Main() {} }");
		}

		private string PreprocessScript(string script)
		{
			var filePath = @"C:\myscript.cs";
			WriteFile(filePath, script);
			return PreprocessFile(filePath);
		}

		private void WriteFile(string filePath, string script)
		{
			_fileSystem.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(script));
		}

		private string PreprocessFile(string filePath)
		{
			var preprocessor = new ScriptPreprocessor(_fileSystem);
			return preprocessor.ProcessFile(filePath, new string[0]);
		}
	}
}
