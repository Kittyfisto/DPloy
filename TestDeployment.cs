using DPloy.Core.PublicApi;

// This is the main entry point of a deployment script
void Main(IDistributor distributor)
{
	var node = distributor.ConnectTo("192.168.0.1", 12345);
}
