// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;
using System.Reflection;
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

			var listener = new TcpListener(IPAddress.Any, ServerConfiguration.Port);
			listener.Start();
			while (true)
			{
				TcpClient client = listener.AcceptTcpClient();
				Logger.LogInformation("Accepted new Client HASH: {ID}", client.GetHashCode());
			}
		}
	}
}