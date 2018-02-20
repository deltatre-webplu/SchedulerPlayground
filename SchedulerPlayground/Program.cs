using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace SchedulerPlayground
{
	class Program
	{
		private static void Main()
		{
			var scheduler = new Scheduler(WaitThreeSecondsAsync);
			WriteLine("Welcome to webplu scheduler playground. Press enter to start.");
			ReadLine();

			scheduler.Start(1000, 1000);
			WriteLine("Scheduler has been started. Press enter to stop it.");
			WriteLine();

			ReadLine();

			scheduler.Stop();
			WriteLine("Press 'R' key to restart the scheduler or anything else to close the program.");

			var key = ReadKey();
			if (key.Key == ConsoleKey.R)
			{
				scheduler.Restart(2000, 1000);

				WriteLine("\nScheduler has been restarted. Press enter to stop it.");
				WriteLine();

				ReadLine();

				scheduler.Dispose();
			}
			else
			{
				scheduler.Dispose();
			}
		}

		private static async Task WaitThreeSecondsAsync(CancellationToken token)
		{
			WriteLine($"Start awaiting delay. {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
			await Task.Delay(3000, token).ConfigureAwait(false);
			WriteLine($"Delay expired. {DateTime.Now.ToString(CultureInfo.InvariantCulture)}");
			WriteLine();
		}
	}
}
