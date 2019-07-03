using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPloy.Core.SharpRemoteInterfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IRegistry
	{
		string GetStringValue(string keyName, string valueName);

		uint GetDwordValue(string keyName, string valueName);
	}
}
