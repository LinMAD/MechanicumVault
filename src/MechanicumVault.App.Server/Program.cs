using System.Reflection;
using MechanicumVault.App.Server.Infrastructure.Transports;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Server
{
	public class Program
	{
		#region Client initilization

		private static ILogger Logger = null!;
		private static IConfiguration Configuration = null!;
		private static ServerConfiguration? ServerConfiguration;

		private static void Initialization(string[] args)
		{
			Configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddCommandLine(args)
				.Build();

			Logger = LoggerFactory
				.Create(builder =>
				{
					builder.AddConfiguration(Configuration.GetSection("Logging"));
					builder.AddConsole();
				})
				.CreateLogger(Assembly.GetExecutingAssembly().GetName().Name ?? "Server");

			ServerConfiguration = Configuration.GetSection("Server").Get<ServerConfiguration>();
		}

		#endregion

		static void Main(string[] args)
		{
			Initialization(args);
			if (ServerConfiguration == null)
			{
				throw new InvalidConfiguration("Missing server configuration for server.");
			}

			Logger.LogInformation("IP: {ServerIP}", ServerConfiguration.Ip);
			Logger.LogInformation("Port: {ServerPort}", ServerConfiguration.Port);
			
			var listener = new TcpTransportPort(Logger, ServerConfiguration);
			listener.Connect();

			while (true) // TODO Make runtime bool state
			{
				try
				{
					var tcpClient = listener.AcceptTcpClient();
					Logger.LogInformation(
						"Accepted new Client Type: {Type} Client HASH: {ID}",
						tcpClient.GetType(),
						tcpClient.GetHashCode()
					);
				}
				catch (Exception e) when (e is RuntimeException or IncomingClientException)
				{
					// There was issue to accept connection, not critical.
				}
				catch (Exception e)
				{
					throw new RuntimeException("An unexpected server error occurred while handling an incoming client connection", e);
				}
			}
		}
	}
}