using System.Net.Sockets;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Providers.Synchronization;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Client.Common.Transports;

/// <summary>
/// TcpTransport responsible to connect to server and send updates.
/// </summary>
public class TcpTransport(ILogger logger, ServerConfiguration cfg)
{
	TcpClient? _tcpClient;

	public void Connect()
	{
		try
		{
			_tcpClient = new TcpClient(cfg.Ip, cfg.Port);
		}
		catch (SocketException e)
		{
			logger.LogError("Connection failed, error: {msg}", e.Message);
			throw new RuntimeException("Unable to connect to server");
		}
		catch (Exception e)
		{
			logger.LogCritical(e, "Failed to connect to server IP: {IP} and Port: {Port}", cfg.Ip, cfg.Port);
			throw new RuntimeException("Unable to connect to server");
		}
	}

	public void Close()
	{
		try
		{
			_tcpClient?.Close();
		}
		catch (Exception e)
		{
			logger.LogWarning("Failed to disconnect from server with error: {msg}", e.Message);
		}
	}

	public void NotifyServer(SynchronizationChangeType syncChangeType, string filePath)
	{
		try
		{
			NetworkStream? netStream = _tcpClient?.GetStream();
		}
		catch (InvalidOperationException e)
		{
			logger.LogCritical("Unable to open channel to server, error: {msg}", e.Message);
			Close();
			throw new RuntimeException("Connection lost or it was closed by server, reconnection to server required.");
		}
		catch (Exception e)
		{
			logger.LogWarning(
				"Failed to notify server about synchronization On {Event} for file {path}, error: {msg}",
				syncChangeType,
				filePath, 
				e.Message
			);
		}
	}
}