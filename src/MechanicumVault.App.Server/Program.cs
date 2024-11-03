namespace MechanicumVault.App.Server
{
	public class Program
	{
		private static Common.Server Server = null!;
		private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		
		static void Main(string[] args)
		{
			// Listen application cancellation for graceful shutdown
			Console.CancelKeyPress += OnExit;
			AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

			Server = new Common.Server(args);
			Server.Run(CancellationTokenSource.Token);
		}
		
		/// <summary>
		/// Handles Ctrl+C (SIGINT) int terminal
		/// </summary>
		private static void OnExit(object? sender, ConsoleCancelEventArgs args)
		{
			args.Cancel = true;
			Server.Stop();
			CancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Handles process termination
		/// </summary>
		private static void OnProcessExit(object? sender, EventArgs args)
		{
			Server.Stop();
			CancellationTokenSource.Cancel();
		}
	}
}