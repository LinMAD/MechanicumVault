namespace MechanicumVault.Core.Configurations;

/// <summary>
/// Application related server configuration.
/// </summary>
public class ServerConfiguration
{
	public string Ip { get; set; } = "127.0.0.1";
	public required int Port { get; set; } = 52000;
}