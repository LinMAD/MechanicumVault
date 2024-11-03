namespace MechanicumVault.App.Client
{
	/// <summary>
	/// File Synchronization CLI Client application.
	/// </summary>
	public class Program
	{
		private static Common.Client Client = null!;
		private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		
		static async Task Main(string[] args)
		{
			Client = new Common.Client(args);

			// Listen application cancellation for graceful shutdown
			Console.CancelKeyPress += OnExit;
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

			await Client.Run(CancellationTokenSource.Token);
		}
		
		/// <summary>
		/// Handles Ctrl+C (SIGINT) int terminal
		/// </summary>
		private static void OnExit(object? sender, ConsoleCancelEventArgs args)
		{
			args.Cancel = true;
			Client.Stop();
			CancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Handles process termination
		/// </summary>
		private static void OnProcessExit(object? sender, EventArgs args)
		{
			Client.Stop();
			CancellationTokenSource.Cancel();
		}
	}
}