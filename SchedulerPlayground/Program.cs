using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace SchedulerPlayground
{
	class Program
	{
		private static void Main(string[] args)
		{
			var scheduler = new Scheduler(WaitFiveSecondsAsync);
			WriteLine("Welcome to webplu scheduler playground. Press a key to start.");
			ReadLine();

			scheduler.Start(2000, 1000);
			WriteLine("Scheduler has been started. Press a key to stop the stop the scheduler.");
			WriteLine();

			ReadLine();
			scheduler.Dispose();
		}

		private static async Task WaitFiveSecondsAsync(CancellationToken token)
		{
			WriteLine($"Start awaiting delay. {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
			await Task.Delay(5000, token).ConfigureAwait(false);
			WriteLine($"Delay expired. {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
			WriteLine();
		}
	}
}
