using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using DPloy.Core;
using DPloy.Core.SharpRemoteImplementations;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Node.SharpRemoteImplementations;
using log4net;
using SharpRemote;
using SharpRemote.ServiceDiscovery;

namespace DPloy.Node
{
	/// <summary>
	///     Responsible for allowing a software distributor
	/// </summary>
	/// <remarks>
	///     This is the counterpart of the 'NodeClient' class in the DPloy.Distributor project.
	/// </remarks>
	/// <remarks>
	///     TODO: Introduce audit log which captures all commands from all distributors EVER, maybe use IsabelDb for this.. (or a plain text file)
	///     TODO: Configuration via app.config file: Only allow certain computers to distribute software to this node: Use a challenge response algorithm to prevent replay attacks
	/// </remarks>
	public sealed class NodeServer
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Interfaces _interfaces;
		private readonly Files _files;
		private readonly Services _services;
		private readonly Shell _shell;
		private readonly Processes _processes;
		private readonly Network _network;
		private readonly Registry _registry;
		private readonly SocketEndPoint _socket;
		private readonly DefaultTaskScheduler _taskScheduler;
		private readonly Filesystem _filesystem;

		public NodeServer()
			: this(null, null, new []{Environment.MachineName})
		{
		}

		public NodeServer(string serviceName, INetworkServiceDiscoverer networkServiceDiscoverer, IEnumerable<string> allowedMachineNames)
		{
			LogAllowedHosts(allowedMachineNames);

			_socket = new SocketEndPoint(EndPointType.Server,
				serviceName,
				clientAuthenticator: MachineNameAuthenticator.CreateForServer(allowedMachineNames.ToArray()),
				networkServiceDiscoverer: networkServiceDiscoverer,
				heartbeatSettings: new HeartbeatSettings
				{
					AllowRemoteHeartbeatDisable = true
				});
			_socket.OnDisconnected += SocketOnOnDisconnected;

			_taskScheduler = new DefaultTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);

			_interfaces = new Interfaces();
			_socket.CreateServant<IInterfaces>(ObjectIds.Interface, _interfaces);

			_files = new Files(_filesystem);
			_socket.CreateServant<IFiles>(ObjectIds.File, _files);

			_shell = new Shell();
			_socket.CreateServant<IShell>(ObjectIds.Shell, _shell);

			_services = new Services();
			_socket.CreateServant<IServices>(ObjectIds.Services, _services);

			_processes = new Processes();
			_socket.CreateServant<IProcesses>(ObjectIds.Processes, _processes);

			_network = new Network();
			_socket.CreateServant<INetwork>(ObjectIds.Network, _network);

			_registry = new Registry();
			_socket.CreateServant<IRegistry>(ObjectIds.Registry, _registry);
		}

		private void LogAllowedHosts(IEnumerable<string> allowedMachineNames)
		{
			var builder = new StringBuilder();
			builder.AppendLine(
				"The following list of hosts are allowed to remotely deploy software & execute commands:");
			builder.Append(string.Join("\r\n", allowedMachineNames.Select(x => $"\t{x}")));
			Log.Info(builder);
		}

		#region IDisposable

		public void Dispose()
		{
			// These objects shall be kept alive at least until this method is called!
			// DO NOT REMOVE THE FOLLOWING CODE
			GC.KeepAlive(_interfaces);
			GC.KeepAlive(_files);
			GC.KeepAlive(_shell);
			GC.KeepAlive(_services);
			GC.KeepAlive(_processes);
			GC.KeepAlive(_network);
			GC.KeepAlive(_registry);

			_socket?.Dispose();
			_taskScheduler?.Dispose();
		}

		#endregion

		private void SocketOnOnDisconnected(EndPoint arg1, ConnectionId arg2)
		{
			_files.CloseAll();
		}

		public void Bind(IPEndPoint ipEndPoint)
		{
			_socket.Bind(ipEndPoint);
		}

		public IPEndPoint Bind(IPAddress ipEndPoint)
		{
			_socket.Bind(ipEndPoint);
			return _socket.LocalEndPoint;
		}
	}
}