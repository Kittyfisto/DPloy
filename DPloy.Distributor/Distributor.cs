using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using DPloy.Core.PublicApi;
using DPloy.Core.SharpRemoteImplementations;
using DPloy.Distributor.Exceptions;
using DPloy.Distributor.Output;

namespace DPloy.Distributor
{
	internal sealed class Distributor
		: IDistributor
		, IDisposable
	{
		private readonly Shell _shell;
		private readonly Files _files;
		private readonly List<NodeClient> _clients;
		private readonly IOperationTracker _operationTracker;
		private readonly DefaultTaskScheduler _taskScheduler;
		private readonly Filesystem _filesystem;

		public Distributor(IOperationTracker operationTracker)
		{
			_taskScheduler = new DefaultTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);

			_shell = new Shell();
			_files = new Files(_filesystem);
			_operationTracker = operationTracker;
			_clients = new List<NodeClient>();
		}

		#region IDisposable

		public void Dispose()
		{
			foreach (var client in _clients) client.Dispose();

			_taskScheduler?.Dispose();
		}

		#endregion

		public INode ConnectTo(string address, TimeSpan connectTimeout)
		{
			return AddClient(() => NodeClient.Create(_operationTracker, address, connectTimeout));
		}

		public INode ConnectTo(IPEndPoint ep)
		{
			return AddClient(() => NodeClient.Create(_operationTracker, ep));
		}

		private INode AddClient(Func<NodeClient> fn)
		{
			NodeClient client = null;
			try
			{
				client = fn();
				_clients.Add(client);
				return client;
			}
			catch (Exception)
			{
				if (client != null)
				{
					client.Dispose();
					_clients.Remove(client);
				}
				throw;
			}
		}

		public void Execute(string clientFilePath, string commandLine = null, TimeSpan? timeout = null, bool printStdOutOnFailure = true, string operationName = null)
		{
			var operation = _operationTracker.BeginExecute(clientFilePath, commandLine, operationName);
			try
			{
				var output = _shell.StartAndWaitForExit(clientFilePath, commandLine, timeout ?? TimeSpan.FromMilliseconds(-1), printStdOutOnFailure);
				if (output.ExitCode != 0)
					throw new ProcessReturnedErrorException(clientFilePath, output, printStdOutOnFailure);

				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}

		public void DeleteFiles(string wildcardPattern)
		{
			var operation = _operationTracker.BeginDeleteFile(wildcardPattern);
			try
			{
				_files.DeleteFilesAsync(wildcardPattern).Wait();
				operation.Success();
			}
			catch (Exception e)
			{
				operation.Failed(e);
				throw;
			}
		}
	}
}