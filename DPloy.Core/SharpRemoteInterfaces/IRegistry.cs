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
