using System;
using System.Net;
using DPloy.Core;
using DPloy.Core.SharpRemoteInterfaces;
using DPloy.Node.SharpRemoteImplementations;
using SharpRemote;

namespace DPloy.Node
{
	public sealed class Node
		: IDisposable
	{
		private readonly IServant _fileServant;
		private readonly IServant _pathsServant;
		private readonly IServant _shellServant;
		private readonly SocketEndPoint _socket;

		public Node()
		{
			_socket = new SocketEndPoint(EndPointType.Server);
			_pathsServant = _socket.CreateServant<IPaths>(ObjectIds.Paths, new Paths());
			_fileServant = _socket.CreateServant<IFile>(ObjectIds.File, new File());
			_shellServant = _socket.CreateServant<IShell>(ObjectIds.Shell, new Shell());
		}

		#region IDisposable

		public void Dispose()
		{
			// These objecs shall be kept alive at least until this method is called!
			GC.KeepAlive(_pathsServant);
			GC.KeepAlive(_fileServant);
			GC.KeepAlive(_shellServant);

			_socket?.Dispose();
		}

		#endregion

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