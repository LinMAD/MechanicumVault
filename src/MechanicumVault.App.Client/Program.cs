using System.Net.Sockets;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Client
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
				.CreateLogger("MechanicumVault.App.Client");

			ServerConfiguration = Configuration.GetSection("Server").Get<ServerConfiguration>();
		}

		#endregion

		static void Main(string[] args)
		{
			Initialization(args);
			if (ServerConfiguration == null)
			{
				throw new InvalidConfiguration("Missing server configuration for client.");
			}

			Logger.LogInformation("IP: {ServerIP}", ServerConfiguration.Ip);
			Logger.LogInformation("Port: {ServerPort}", ServerConfiguration.Port);

			var tcp = new TcpClient(ServerConfiguration.Ip, ServerConfiguration.Port);
			var stream = tcp.GetStream();
		}
	}
}