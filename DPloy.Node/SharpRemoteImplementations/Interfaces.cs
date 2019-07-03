using DPloy.Core.SharpRemoteInterfaces;
using SharpRemote;

namespace DPloy.Node.SharpRemoteImplementations
{
	internal sealed class Interfaces
		: IInterfaces
	{
		#region Implementation of IManagement

		public TypeModel GetTypeModel()
		{
			var typeModel = new TypeModel();
			typeModel.Add<IInterfaces>(assumeByReference: true);
			typeModel.Add<IFiles>(assumeByReference: true);
			typeModel.Add<IShell>(assumeByReference: true);
			typeModel.Add<IServices>(assumeByReference: true);
			typeModel.Add<IProcesses>(assumeByReference: true);
			typeModel.Add<INetwork>(assumeByReference: true);
			typeModel.Add<IRegistry>(assumeByReference: true);
			return typeModel;
		}

		#endregion
	}
}
