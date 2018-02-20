using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulerPlayground.Extensions
{
	public static class SchedulerExtensions
	{
		/// <exception cref="ArgumentNullException">Throws when parameter scheduler is null</exception>
		/// <exception cref="ArgumentNullException">Throws when parameter actionToBeScheduled is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">Throws when parameter dueTimeMs is negative and other than -1</exception>
		/// <exception cref="ArgumentOutOfRangeException">Throws when parameter periodMs is negative and other than -1</exception>
		public static IDisposable ScheduleRecurringAction(
			this IScheduler scheduler,
			int dueTimeMs,
			int periodMs,
			Func<CancellationToken, Task> asyncAction,
			CancellationToken token = default(CancellationToken),
			Action<Exception> onError = null)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			if (asyncAction == null)
				throw new ArgumentNullException(nameof(asyncAction));

			if (!IsValidTimeInterval(dueTimeMs))
				throw new ArgumentOutOfRangeException(nameof(dueTimeMs), $"Invalid time interval in milliseconds: {dueTimeMs}");

			if (!IsValidTimeInterval(periodMs))
				throw new ArgumentOutOfRangeException(nameof(periodMs), $"Invalid time interval in milliseconds: {periodMs}");

			var dueTimeTimeSpan = TimeSpan.FromMilliseconds(dueTimeMs);
			var periodTimeTimeSpan = TimeSpan.FromMilliseconds(periodMs);
			var errorHandler = onError ?? TraceError;

			if (dueTimeMs == Timeout.Infinite)
			{
				return scheduler.Schedule(TimeSpan.FromMilliseconds(0), _ => { });
			}

			return scheduler.Schedule(dueTimeTimeSpan, async scheduleNext =>
			{
				try
				{
					await asyncAction(token).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					//Cancellation of async action has been requested from outside
				}
				catch (Exception exception)
				{
					errorHandler(exception);
				}

				if (periodMs != Timeout.Infinite)
				{
					scheduleNext(periodTimeTimeSpan);
				}
			});

			void TraceError(Exception exception) =>
				Trace.TraceError($"An error occurred while running scheduled action: {exception}");

			bool IsValidTimeInterval(int milliseconds) =>
				milliseconds >= 0 || milliseconds == Timeout.Infinite;
		}
	}
}
