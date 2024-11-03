using System.Reflection;
using MechanicumVault.App.Client.Common.Configurations;
using MechanicumVault.App.Client.Common.Mode;
using MechanicumVault.App.Client.Common.Transports;
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
	private readonly TcpTransport _tcpTransport;

	#region Configurations

	private readonly ServerConfiguration _serverConfiguration = null!;
	private readonly ApplicationConfiguration _applicationConfiguration;

	#endregion

	private bool isClientRunning = false;

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
		
		_tcpTransport = new TcpTransport(Logger, _serverConfiguration);
	}

	public void Run()
	{
		try
		{
			var synchronizationProvider = GetStorageSynchronizationProvider(_applicationConfiguration.SourceMode);
			synchronizationProvider.Observe();
			
			isClientRunning = true;
		}
		catch (InvalidConfiguration e)
		{
			Logger.LogError("Failed to observ source directory, error: {err}", e.Message);
			return;
		}
		
		try
		{
			Logger.LogInformation("Connecting to Server: {ServerIP}:{ServerPort} ...", _serverConfiguration.Ip,  _serverConfiguration.Port);
			_tcpTransport.Connect();
			Logger.LogInformation("Connected...");

			while (isClientRunning)
			{
				// Give small delay if on client side there is activity before doing any sync
				Thread.Sleep(1000); 
			}
		}
		catch (Exception e)
		{
			Logger.LogError(e, "An error occured while running the client");
			throw new RuntimeException("Execution of application aborted with unexpected internal error.");
		}
	}

	public void Stop()
	{
		// TODO Update ISynchronizationProvider to gracefully stop File Observing
		_tcpTransport.Close();
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
		
		_tcpTransport.NotifyServer(e.ChangeType, e.FilePath);
	}
}