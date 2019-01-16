using System;

namespace DPloy.Core.PublicApi
{
	public interface IClient : IDisposable
	{
		void CopyFile(string sourceFilePath, string destinationPath);
		void Install(string installerPath);
		void ExecuteFile(string clientFilePath);
		int ExecuteCommand(string cmd);
	}
}