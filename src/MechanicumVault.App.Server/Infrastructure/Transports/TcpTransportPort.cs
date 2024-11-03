using System.Net;
using System.Net.Sockets;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Transports;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Server.Infrastructure.Transports;

/// <summary>
/// TcpTransportPort responsible for listening TcpTransportAdapter that will notify with updates.
/// </summary>
public class TcpTransportPort(ILogger logger, ServerConfiguration serverCfg) : ITcpTransport
{
	private TcpListener? _tcpListener;
	
	public void Connect()
	{
		try
		{
			_tcpListener = new TcpListener(IPAddress.Any, serverCfg.Port);
			_tcpListener.Start();
		}
		catch (SocketException e)
		{
			logger.LogCritical("Failed to listen connections, error: {msg}", e.Message);
			throw new RuntimeException("Unable establish connect listening to receive messages.");
		}
		catch (Exception e)
		{
			logger.LogCritical("Failed to establish TCP Transport listener, error: {msg}", e.Message);
			throw new RuntimeException("Unable establish connect listening to receive messages.");
		}
	}

	public void Close()
	{
		try
		{
			_tcpListener?.Stop();
		}
		catch (Exception e)
		{
			logger.LogWarning("Failed to disconnect from server with error: {msg}", e.Message);
		}
	}

	public TcpClient AcceptTcpClient()
	{
		try
		{
			var incomingTcpClient = _tcpListener?.AcceptTcpClient();
			if (incomingTcpClient == null)
			{
				throw new RuntimeException("Failed to accept tcp client.");
			}

			return incomingTcpClient;
		}
		catch (Exception e)
		{
			logger.LogWarning("Failed to disconnect from server with error: {msg}", e.Message);
			throw new IncomingClientException("Failed to accept tcp client", e);
		}
	}
}