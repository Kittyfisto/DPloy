using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DPloy.Node
{
	public static class AssemblyLoader
	{
		private static readonly Dictionary<Assembly, string> AssemblyLocations = new Dictionary<Assembly, string>();

		/// <summary>
		///     Referenced assemblies will from now on be loaded from files embedded in the given assembly, before
		///     the file system or the GAC is tried.
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="prefix"></param>
		public static void LoadAssembliesFrom(Assembly assembly, string prefix)
		{
			AssemblyLocations.Add(assembly, prefix);

			AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
		}

		private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var extensions = new[] {"exe", "dll"};
			var assemblyName = new AssemblyName(args.Name);
			var fileNames = extensions.Select(extension => $"{assemblyName.Name}.{extension}").ToList();
			foreach (var fileName in fileNames)
			{
				if (TryLoadEmbeddedAssembly(fileName, out var assembly))
				{
					return assembly;
				}
			}

			return null;
		}

		private static bool TryLoadEmbeddedAssembly(string fileName, out Assembly assembly)
		{
			foreach (var location in AssemblyLocations)
			{
				if (TryLoadEmbeddedAssemblyFrom(fileName, location, out assembly))
				{
					return true;
				}
			}

			assembly = null;
			return false;
		}

		private static bool TryLoadEmbeddedAssemblyFrom(string fileName, KeyValuePair<Assembly, string> location, out Assembly assembly)
		{
			var name = $"{location.Value}\\{fileName}";
			var stream = location.Key.GetManifestResourceStream(name);
			if (stream == null)
			{
				assembly = null;
				return false;
			}

			using (var buffer = new MemoryStream())
			{
				stream.CopyTo(buffer);
				assembly = Assembly.Load(buffer.ToArray());
				return true;
			}
		}
	}
}