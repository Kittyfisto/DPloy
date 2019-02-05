using System.Collections.Generic;
using System.Linq;

namespace DPloy.Distributor.Output
{
	internal sealed class NodeTracker
	{
		private readonly ConsoleWriter _consoleWriter;
		private readonly IReadOnlyList<string> _nodes;
		private readonly Dictionary<string, OperationTracker> _operationTrackers;
		private readonly object _syncRoot;

		public NodeTracker(ConsoleWriter consoleWriter, IReadOnlyList<string> nodes)
		{
			_consoleWriter = consoleWriter;
			_nodes = nodes;
			_operationTrackers = new Dictionary<string, OperationTracker>();
			_syncRoot = new object();
		}

		public OperationTracker Get(string nodeAddress)
		{
			var tracker = new OperationTracker();
			lock (_syncRoot)
			{
				_operationTrackers.Add(nodeAddress, tracker);
			}
			return tracker;
		}

		public void ThrowOnFailure()
		{
			IReadOnlyList<OperationTracker> trackers;
			lock (_syncRoot)
			{
				trackers = _operationTrackers.Values.ToList();
			}

			foreach (var tracker in trackers)
			{
				tracker.ThrowOnFailure();
			}
		}
	}
}