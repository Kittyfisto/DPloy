using System;
using System.Net;
using DPloy.Core;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Node.SharpRemoteImplementations;
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
		private readonly Files _files;
		private readonly Services _services;
		private readonly Shell _shell;
		private readonly SocketEndPoint _socket;

		public NodeServer()
			: this(serviceName: null, networkServiceDiscoverer: null)
		{
		}

		public NodeServer(string serviceName, INetworkServiceDiscoverer networkServiceDiscoverer)
		{
			_socket = new SocketEndPoint(EndPointType.Server,
			                             serviceName,
			                             networkServiceDiscoverer: networkServiceDiscoverer,
			                             heartbeatSettings: new HeartbeatSettings
			                             {
				                             AllowRemoteHeartbeatDisable = true
			                             });
			_socket.OnDisconnected += SocketOnOnDisconnected;

			_files = new Files();
			_socket.CreateServant<IFiles>(ObjectIds.File, _files);

			_shell = new Shell();
			_socket.CreateServant<IShell>(ObjectIds.Shell, _shell);

			_services = new Services();
			_socket.CreateServant<IServices>(ObjectIds.Services, _services);
		}

		#region IDisposable

		public void Dispose()
		{
			// These objects shall be kept alive at least until this method is called!
			GC.KeepAlive(_files);
			GC.KeepAlive(_shell);
			GC.KeepAlive(_services);

			_socket?.Dispose();
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