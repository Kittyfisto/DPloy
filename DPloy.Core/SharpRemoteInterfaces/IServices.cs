namespace DPloy.Core.SharpRemoteInterfaces
{
	public interface IServices
	{
		void Start(string serviceName);
		void Stop(string serviceName);
	}
}
