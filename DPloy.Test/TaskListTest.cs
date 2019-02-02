using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DPloy.Distributor;
using FluentAssertions;
using NUnit.Framework;

namespace DPloy.Test
{
	[TestFixture]
	public sealed class TaskListTest
	{
		[Test]
		public void TestAddCompleted()
		{
			var list = new TaskList(1);
			var task = Task.FromResult(42);

			list.Add(task);
			new Action(() => list.WaitAll()).Should().NotThrow();
		}

		[Test]
		public void TestAddManyCompleted()
		{
			var list = new TaskList(1);

			for (int i = 0; i < 1000; ++i)
			{
				var task = Task.FromResult(42);
				list.Add(task);
			}

			new Action(() => list.WaitAll()).Should().NotThrow();
		}

		[Test]
		public void TestAddMany()
		{
			var list = new TaskList(3);
			var tasks = new List<Task>();

			for (int i = 0; i < 1000; ++i)
			{
				var task = Task.Factory.StartNew(() => Thread.Sleep(TimeSpan.FromMilliseconds(1)));
				tasks.Add(task);
				list.Add(task);
			}

			new Action(() => list.WaitAll()).Should().NotThrow();
			foreach (var task in tasks)
			{
				task.IsCompleted.Should().BeTrue();
			}
		}

		[Test]
		public void TestAddFailed()
		{
			var list = new TaskList(1);
			var task = Task.FromException(new NotImplementedException());

			new Action(() => list.Add(task)).Should().NotThrow();
			new Action(() => list.WaitAll()).Should()
			                                .Throw<AggregateException>()
			                                .WithInnerException<NotImplementedException>();
		}
	}
}
