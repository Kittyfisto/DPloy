using SharpRemote;

namespace DPloy.Core.SharpRemoteInterfaces
{
	/// <summary>
	///     Responsible for offering a description of the supported interfaces and types.
	/// </summary>
	public interface IInterfaces
	{
		TypeModel GetTypeModel();
	}
}