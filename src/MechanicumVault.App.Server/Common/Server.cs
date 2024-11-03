using System.Net.Sockets;
using System.Reflection;
using MechanicumVault.App.Server.Common.Configurations;
using MechanicumVault.App.Server.Infrastructure.Transports;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Server.Common;

/// <summary>
/// Server application.
/// </summary>
public class Server
{
	private static ILogger Logger = null!;

	private readonly IConfiguration _configuration = null!;
	private readonly ServerConfiguration _serverConfiguration = null!;
	private readonly ApplicationConfiguration _applicationConfiguration = null!;
	
	private TcpTransportPort? _tcpTransportPortListener;
	
	private bool _isServerRunning = false;

	public Server(string[] commandLineArguments)
	{
		_configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddCommandLine(commandLineArguments)
			.Build();

		Logger = LoggerFactory
			.Create(builder =>
			{
				builder.AddConfiguration(_configuration.GetSection("Logging"));
				builder.AddConsole();
			})
			.CreateLogger(Assembly.GetExecutingAssembly().GetName().Name ?? "Server");

		var serverCfg = _configuration.GetSection("Server").Get<ServerConfiguration>();
		var appCfg = _configuration.GetSection("Application").Get<ApplicationConfiguration>();
		_serverConfiguration = serverCfg ?? throw new InvalidConfiguration(
			$"Missing server configuration for server, please check your appsettings.json - {nameof(ServerConfiguration)}"
		);
		_applicationConfiguration = appCfg ?? throw new InvalidConfiguration(
			$"Missing server configuration for file distinction storage, please check your appsettings.json - {nameof(ApplicationConfiguration)}"
		);
	}

	public void Run(CancellationToken cancellationToken)
	{
		Logger.LogInformation("Starting server...");
		
		_tcpTransportPortListener = new TcpTransportPort(Logger, _serverConfiguration, _applicationConfiguration);
		_tcpTransportPortListener.Connect();
		_isServerRunning = _tcpTransportPortListener.IsAwaitingClients();
		
		Logger.LogInformation("Server ready to accept client connections...");
		
		while (!cancellationToken.IsCancellationRequested || _isServerRunning)
		{
			var newTcpClient = _tcpTransportPortListener.AcceptTcpClient();

			Logger.LogInformation(
				"Accepted new Client Type: {Type} Client HASH: {ID} and Endpoint: {Endpoint}",
				newTcpClient.GetType(),
				newTcpClient.GetHashCode(),
				newTcpClient.Client.RemoteEndPoint?.ToString()
			);
			
			Thread clientThread = new Thread(() => _tcpTransportPortListener.HandleClientNotification(newTcpClient));
			clientThread.Start();
		}
	}

	public void Stop()
	{
		Logger.LogInformation("Stopping server...");

		_isServerRunning = false;
		if (_tcpTransportPortListener != null && _tcpTransportPortListener.IsAwaitingClients())
		{
			_tcpTransportPortListener.Close();	
		}
	}
}