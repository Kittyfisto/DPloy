using System;
using System.ComponentModel;
using System.Reflection;
using System.ServiceProcess;
using DPloy.Core.SharpRemoteInterfaces;
using log4net;

namespace DPloy.Core.SharpRemoteImplementations
{
	public sealed class Services
		: IServices
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void Start(string serviceName)
		{
			Log.DebugFormat("Starting service '{0}'...", serviceName);

			try
			{
				using (var controller = new ServiceController(serviceName))
				{
					controller.Start();
				}
			}
			catch (InvalidOperationException e)
			{
				if (IsAlreadyStartedException(e))
				{
					Log.InfoFormat("Service '{0}' is already running - nothing needs to be done", serviceName);
					Log.DebugFormat("Ignoring exception: '{0}'", e);
					return;
				}

				// we can't just swallow every exception
				throw new ArgumentException($"No such service: {serviceName}", e);
			}

			Log.InfoFormat("Started service '{0}'", serviceName);
		}

		public void Stop(string serviceName)
		{
			Log.DebugFormat("Stopping service '{0}'...", serviceName);

			try
			{
				using (var controller = new ServiceController(serviceName))
				{
					controller.Stop();
				}
			}
			catch (InvalidOperationException e)
			{
				if (IsNotRunningException(e))
				{
					Log.InfoFormat("Service '{0}' is not running - nothing needs to be done", serviceName);
					Log.DebugFormat("Ignoring exception: '{0}'", e);
					return;
				}

				Log.InfoFormat("Service '{0}' does not exist so it doesn't need to be stopped anyways", serviceName);
				Log.DebugFormat("Ignoring exception: '{0}'", e);
				return;
			}

			Log.InfoFormat("Stopped service '{0}'", serviceName);
		}

		private static bool IsAlreadyStartedException(InvalidOperationException exception)
		{
			var inner = exception?.InnerException as Win32Exception;
			if (inner == null)
				return false;

			// ERROR_SERVICE_ALREADY_RUNNING, https://docs.microsoft.com/en-us/windows/desktop/debug/system-error-codes--1000-1299-
			return inner.NativeErrorCode == 1056;
		}

		private bool IsNotRunningException(InvalidOperationException exception)
		{
			var inner = exception?.InnerException as Win32Exception;
			if (inner == null)
				return false;

			// ERROR_SERVICE_NOT_ACTIVE, https://docs.microsoft.com/en-us/windows/desktop/debug/system-error-codes--1000-1299-
			return inner.NativeErrorCode == 1062;
		}
	}
}
