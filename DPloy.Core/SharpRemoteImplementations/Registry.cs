using System;
using System.Reflection;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;
using Microsoft.Win32;

namespace DPloy.Core.SharpRemoteImplementations
{
	public sealed class Registry
		: IRegistry
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public string GetStringValue(string keyName, string valueName)
		{
			var value = GetValue(keyName, valueName);
			return (string)value;
		}

		public uint GetDwordValue(string keyName, string valueName)
		{
			var value = GetValue(keyName, valueName);
			return (uint)value;
		}

		private static object GetValue(string keyName, string valueName)
		{
			Log.DebugFormat("Retrieving {0} {1}...", keyName, valueName);

			const string hklm = @"HKEY_LOCAL_MACHINE\";
			object value;
			if (keyName.StartsWith(hklm))
			{
				using (RegistryKey localKey = OpenHKLM())
				{
					var subKeyName = keyName.Substring(hklm.Length);
					using (var subKey = localKey.OpenSubKey(subKeyName))
					{
						value = subKey?.GetValue(valueName);
					}
				}
			}
			else
			{
				value = Microsoft.Win32.Registry.GetValue(keyName, valueName, null);
			}

			Log.InfoFormat("Retrieved {0} {1}: {2}", keyName, valueName, value);
			return value;
		}

		private static RegistryKey OpenHKLM()
		{
			if (Environment.Is64BitOperatingSystem)
				return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

			return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
		}
	}
}