using MechanicumVault.App.Client.Common;
using MechanicumVault.App.Server.Common;
using MechanicumVault.EndToEnd.Generators;

namespace MechanicumVault.EndToEnd.Handlers;

/// <summary>
/// IntegrationProcessHandler allows to connect Client application with Server application
/// </summary>
public class IntegrationProcessHandler
{
	private readonly Client _syncClient;
	private readonly Server _syncServer;

	public IntegrationProcessHandler(string clientSourceDirectory, string serverDestinationDirectory)
	{
		var randomPort = PortGenerator.Generate(52000, 52999);
		var clientArgs = new[]
		{
			"--Logging:LogLevel:Default=Debug",
			"--Server:IP=127.0.0.1", 
			$"--Server:Port={randomPort}",
			"--Application:SourceMode=FileSystem",
			$"--Application:SourceDirectory={clientSourceDirectory}"
		};
		_syncClient = new Client(clientArgs);

		var serverArgs = new[]
		{
			"--Logging:LogLevel:Default=Debug", 
			$"--Server:Port={randomPort}",
			$"--Application:DestinationDirectory={serverDestinationDirectory}"
		};
		_syncServer = new Server(serverArgs);
	}

	public async Task StartServerWithClient(CancellationToken cancellationToken)
	{
		var serverTask = Task.Run(() => StartServer(cancellationToken), cancellationToken);
		var clientTask = Task.Run(() => StartClient(cancellationToken), cancellationToken);

		// Await both tasks to ensure they finish before the test completes
		await Task.WhenAll(serverTask, clientTask);
	}

	public void StartClient(CancellationToken cancellationToken)
	{
		_syncClient.Run(cancellationToken);
	}
	
	public void StartServer(CancellationToken cancellationToken)
	{
		_syncServer.Run(cancellationToken);
	}
}