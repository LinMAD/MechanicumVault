using System.Reflection;
using MechanicumVault.App.Client.Common.Configurations;
using MechanicumVault.App.Client.Common.Mode;
using MechanicumVault.App.Client.Infrastructure.Transports;
using MechanicumVault.Core;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Providers.Synchronization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Client.Common;

/// <summary>
/// Client application
/// </summary>
public sealed class Client
{
	private static ILogger Logger = null!;
	private readonly ServiceProvider _serviceProvider;
	private readonly TcpTransportAdapter _tcpTransportAdapter;

	#region Configurations

	private readonly ServerConfiguration _serverConfiguration = null!;
	private readonly ApplicationConfiguration _applicationConfiguration;

	#endregion

	private bool _isClientRunning = false;
	private bool _isTerminationCalled = false;

	/// <summary>
	/// Client initialization with prepared dependencies.
	/// </summary>
	/// <param name="commandLineArguments">terminal arguments</param>
	public Client(string[] commandLineArguments)
	{
		_serviceProvider = DependencyInjection.GetServiceProvider();

		var genericCfg = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddCommandLine(commandLineArguments)
			.Build();
		Logger = LoggerFactory
			.Create(builder =>
			{
				builder.AddConfiguration(genericCfg.GetSection("Logging"));
				builder.AddConsole();
			})
			.CreateLogger(Assembly.GetExecutingAssembly().GetName().Name ?? "Client");

		// TODO Create validator / dispatcher for configurations
		var serverCfg = genericCfg.GetSection("Server").Get<ServerConfiguration>();
		var appCfg = genericCfg.GetSection("Application").Get<ApplicationConfiguration>();
		_serverConfiguration = serverCfg ?? throw new InvalidConfiguration("Unable to load client configuration for server.");
		_applicationConfiguration = appCfg ?? throw new InvalidConfiguration("Unable to load client configuration with source directory path for synchronization.");

		_tcpTransportAdapter = new TcpTransportAdapter(Logger, _serverConfiguration, _applicationConfiguration);
	}

	public void Run(CancellationToken cancellationToken)
	{
		try
		{
			var synchronizationProvider = GetStorageSynchronizationProvider(_applicationConfiguration.SourceMode);
			synchronizationProvider.Observe();

			_isClientRunning = true;
		}
		catch (InvalidConfiguration e)
		{
			Logger.LogError("Failed to observe source directory for file synchronization, error: {err}", e.Message);
			return;
		}

		Logger.LogInformation("File synchronization client ready...");
		while (!cancellationToken.IsCancellationRequested || _isClientRunning)
		{
			try
			{
				Task.Delay(1000, cancellationToken); // Delay to avoid tight looping.
			}
			catch (TaskCanceledException)
			{
				break;
			}
		}
	}

	public void Stop()
	{
		if (_isTerminationCalled) return;

		Logger.LogInformation("Stopping client...");
		_isClientRunning = false;
		_isTerminationCalled = true;
	}

	ISynchronizationProvider GetStorageSynchronizationProvider(ClientProviderMode mode)
	{
		ISynchronizationProvider provider;

		switch (mode)
		{
			case ClientProviderMode.FileSystem:
				var newProvider = _serviceProvider.GetService<ISynchronizationProvider>();
				provider = newProvider ?? throw new RuntimeException("Unable to locate the storage synchronization provider");
				provider.SetNewObservablePointOfInterest(_applicationConfiguration.SourceDirectory);
				break;
			default:
				throw new InvalidConfiguration($"Unexpected {nameof(ClientProviderMode)}: {mode}");
		}

		provider.OnFileChanged += SynchronizationProvider_BindOnFileChanged;

		return provider;
	}

	private void SynchronizationProvider_BindOnFileChanged(object? sender, SynchronizationEvent e)
	{
		if (sender is not ISynchronizationProvider)
		{
			Logger.LogError("The synchronization provider is not supported");
			return;
		}

		Logger.LogDebug("Synchronization provider: {Name}", sender.GetType().Name);
		Logger.LogDebug("Event Type: {Type} - File: {Path}", e.ChangeType, e.FilePath);

		try
		{
			Logger.LogDebug(
				"Establishing new connection to server for synchronization: {IP}:{Port}",
				_serverConfiguration.Ip,
				_serverConfiguration.Port
			);

			_tcpTransportAdapter.Connect();

			Logger.LogDebug("Connection established...");
		}
		catch (Exception ex)
		{
			Logger.LogError(
				"An error occured while establishing connection to server for synchronization, error: {msg}",
				ex.Message
			);
			return;
		}

		_tcpTransportAdapter.NotifyServer(e.ChangeType, e.FilePath);
		_tcpTransportAdapter.Close();
	}
}