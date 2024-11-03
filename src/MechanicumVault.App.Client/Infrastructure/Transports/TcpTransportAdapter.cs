using System.Net.Sockets;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Transports;
using MechanicumVault.Core.Providers.Synchronization;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Client.Infrastructure.Transports;

/// <summary>
/// TcpTransportAdapter responsible to connect to Server TCPTransportPort and send updates.
/// </summary>
public class TcpTransportAdapter(ILogger logger, ServerConfiguration serverCfg) : ITcpTransport
{
	TcpClient? _tcpClient;

	public void Connect()
	{
		try
		{
			_tcpClient = new TcpClient(serverCfg.Ip, serverCfg.Port);
		}
		catch (SocketException e)
		{
			logger.LogCritical("Connection failed, error: {msg}", e.Message);
			throw new RuntimeException("Unable to connect to server");
		}
		catch (Exception e)
		{
			logger.LogCritical(e, "Failed to connect to server IP: {IP} and Port: {Port}", serverCfg.Ip, serverCfg.Port);
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
			// TODO Send file changes
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