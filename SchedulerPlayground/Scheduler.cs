using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SchedulerPlayground.Extensions;

namespace SchedulerPlayground
{
	public sealed class AsyncScheduler : IDisposable
	{
		private static readonly IScheduler Scheduler = System.Reactive.Concurrency.Scheduler.Default;

		private readonly Func<CancellationToken, Task> _asyncAction;
		private readonly object _lock;
		private IDisposable _schedule;
		private CancellationTokenSource _cancellation;
		private bool _isRunning;

		/// <param name="asyncAction">The asynchronous and cancellable action to be scheduled for execution</param>
		/// <exception cref="System.ArgumentNullException">Throws when parameter asyncAction is null</exception>
		public AsyncScheduler(Func<CancellationToken, Task> asyncAction)
		{
			_asyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
			_schedule = null;
			_lock = new object();
			_cancellation = null;
			_isRunning = false;
		}

		/// <summary>
		/// Call this method to start the scheduler. If an attempt is made to start the scheduler when it is already running nothing will happen.
		/// </summary>
		/// <param name="dueTimeMs">The amount of time to delay before invoking the callback for the first time, in milliseconds. Specify Timeout.Infinite to prevent the scheduler from restarting. Specify zero (0) to start the scheduler immediately.</param>
		/// <param name="periodMs">The time interval between two consecutives invocations of the callback. Specify Timeout.Infinite to disable periodic signaling.</param>
		public void Start(int dueTimeMs, int periodMs)
		{
			lock (_lock)
			{
				StartInner(dueTimeMs, periodMs);
			}
		}

		private void StartInner(int dueTimeMs, int periodMs)
		{
			if (_isRunning)
			{
				return;
			}

			_cancellation = new CancellationTokenSource();

			_schedule = Scheduler.ScheduleRecurringAction(
				dueTimeMs,
				periodMs,
				_asyncAction,
				_cancellation.Token);

			_isRunning = true;
		}

		/// <summary>
		/// Call this method to stop the scheduler. If an attempt is made to stop the scheduler when it is not running nothing will happen.
		/// </summary>
		public void Stop()
		{
			lock (_lock)
			{
				StopInner();
			}
		}

		private void StopInner()
		{
			if (!_isRunning)
			{
				return;
			}

			_schedule?.Dispose();
			_schedule = null;

			_cancellation?.Cancel();
			_cancellation?.Dispose();
			_cancellation = null;

			_isRunning = false;
		}

		/// <summary>
		/// Call this method to restart a running scheduler or to start a not running scheduler.
		/// </summary>
		/// <param name="dueTimeMs">The amount of time to delay before invoking the callback for the first time, in milliseconds. Specify Timeout.Infinite to prevent the scheduler from restarting. Specify zero (0) to start the scheduler immediately.</param>
		/// <param name="periodMs">The time interval between two consecutives invocations of the callback. Specify Timeout.Infinite to disable periodic signaling.</param>
		public void Restart(int dueTimeMs, int periodMs)
		{
			lock (_lock)
			{
				if (_isRunning)
				{
					StopInner();
				}

				StartInner(dueTimeMs, periodMs);
			}
		}

		public void Dispose() => Stop();
	}
}
