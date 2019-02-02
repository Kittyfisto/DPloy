using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DPloy.Distributor
{
	/// <summary>
	///     Responsible for holding a list of tasks.
	///     Tasks are added via <see cref="Add" />, which allows for a certain amount of tasks to be held.
	///     As soon as there are as many tasks as was specified in the ctor, the method will block until
	///     one of the existing tasks has finished.
	/// </summary>
	internal sealed class TaskList
	{
		private readonly int _maxPending;
		private readonly List<Task> _pendingTasks;
		private readonly List<Task> _finishedTasks;
		private readonly object _syncRoot;

		public TaskList(int maxPending)
		{
			_pendingTasks = new List<Task>();
			_finishedTasks = new List<Task>();
			_maxPending = maxPending;
			_syncRoot = new object();
		}

		public void Add(Task task)
		{
			while (true)
			{
				if (TryAdd(task))
					return;

				Thread.Sleep(TimeSpan.FromMilliseconds(value: 10));
			}
		}

		public void WaitAll()
		{
			Task[] pending;
			lock (_syncRoot)
			{
				pending = _pendingTasks.ToArray();
			}
			Task.WaitAll(pending);

			Task[] finished;
			lock (_syncRoot)
			{
				finished = _finishedTasks.ToArray();
			}
			Task.WaitAll(finished);
		}

		private bool TryAdd(Task task)
		{
			lock (_syncRoot)
			{
				if (_pendingTasks.Count >= _maxPending)
					return false;

				if (task.IsCompleted || task.IsFaulted)
				{
					_finishedTasks.Add(task);
					return true;
				}

				_pendingTasks.Add(task);
			}

			task.ContinueWith(OnTaskFinished);
			return true;
		}

		private void OnTaskFinished(Task task)
		{
			lock (_syncRoot)
			{
				_pendingTasks.Remove(task);
				_finishedTasks.Add(task);
			}
		}
	}
}