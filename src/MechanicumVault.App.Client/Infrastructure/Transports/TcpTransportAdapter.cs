using System.Net.Sockets;
using System.Security.Cryptography;
using MechanicumVault.App.Client.Common.Configurations;
using MechanicumVault.App.Client.Common.Mode;
using MechanicumVault.App.Client.Extensions;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Transports;
using MechanicumVault.Core.Providers.Synchronization;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Client.Infrastructure.Transports;

/// <summary>
/// TcpTransportAdapter responsible to connect to Server TCPTransportPort and send updates.
/// </summary>
public class TcpTransportAdapter : ITcpTransport
{
	private readonly ILogger _logger;
	private readonly ServerConfiguration _serverCfg;
	private readonly ApplicationConfiguration _appCfg;
	
	// TODO Refactor this part to have better state control when TCP Client is connected to server
	private TcpClient? _tcpClient;
	private bool isTcpClientConnected = false;

	public TcpTransportAdapter(ILogger logger, ServerConfiguration serverCfg, ApplicationConfiguration appCfg)
	{
		_logger = logger;
		_serverCfg = serverCfg;
		_appCfg = appCfg;
	}

	public void Connect()
	{
		try
		{
			_tcpClient = new TcpClient(_serverCfg.Ip, _serverCfg.Port);
			isTcpClientConnected = true;
		}
		catch (SocketException e)
		{
			_logger.LogCritical("Connection failed, error: {msg}", e.Message);
			throw new RuntimeException("Unable to connect to server");
		}
		catch (Exception e)
		{
			_logger.LogCritical(
				e, 
				"Failed to connect to server IP: {IP} and Port: {Port}", 
				_serverCfg.Ip, 
				_serverCfg.Port
			);
			throw new RuntimeException("Unable to connect to server");
		}
	}

	public void Close()
	{
		try
		{
			if (isTcpClientConnected)
			{
				_tcpClient?.Close();				
			}
		}
		catch (Exception e)
		{
			_logger.LogWarning("Failed to disconnect from server with error: {msg}", e.Message);
		}
	}

	public void NotifyServer(SynchronizationChangeType syncChangeType, string filePath)
	{
		try
		{
			NetworkStream? stream = _tcpClient?.GetStream();
			if (stream == null) return;
			
			var newFileSynchronizationMessage = new FileSynchronizationMessage(syncChangeType, filePath.GetSourceFolderRelativePath(_appCfg));
			byte[] bytesNewMessage = newFileSynchronizationMessage.ToBytes();
			if (bytesNewMessage.Length == 0) return;
			
			_logger.LogInformation(
				"Synchronization of file {path} with {event}", 
				newFileSynchronizationMessage.FilePath,
				newFileSynchronizationMessage.SyncChangeType
			);

			stream.Write(bytesNewMessage, 0, bytesNewMessage.Length);
			switch (syncChangeType)
			{
				case SynchronizationChangeType.Created:
				case SynchronizationChangeType.Changed:
				case SynchronizationChangeType.Renamed:
					// Send file content if the file exists
					if (_appCfg.SourceMode.Equals(ClientProviderMode.FileSystem) && File.Exists(filePath))
					{
						byte[] fileData = File.ReadAllBytes(filePath);
						byte[] fileLength = BitConverter.GetBytes(fileData.Length);

						stream.Write(fileLength, 0, fileLength.Length);
						stream.Write(fileData, 0, fileData.Length);

						// Send file hash for integrity check
						byte[] hash = SHA256.HashData(fileData);
						stream.Write(hash, 0, hash.Length);
						_logger.LogInformation(
							"File is submitted to server for synchronization - event {type} for file: {path}",
							newFileSynchronizationMessage.SyncChangeType,
							newFileSynchronizationMessage.FilePath
						);
					}
					break;
				case SynchronizationChangeType.Deleted:
					stream.Write(bytesNewMessage, 0, bytesNewMessage.Length);
					_logger.LogInformation(
						"File: {path} is submitted to be deleted on server side server for synchronization - event {type}",
						newFileSynchronizationMessage.SyncChangeType,
						newFileSynchronizationMessage.FilePath
					);
					break;
				default:
					throw new NotFoundException("Unhandled synchronization change type");
			}
		}
		catch (InvalidOperationException e)
		{
			_logger.LogCritical("Unable to open channel to server, error: {msg}", e.Message);
			throw new RuntimeException("Connection lost or it was closed by server, reconnection to server required.");
		}
		catch (Exception e)
		{
			_logger.LogWarning(
				"Failed to notify server about synchronization On {Event} for file {path}, error: {msg}",
				syncChangeType,
				filePath, 
				e.Message
			);
		}
	}
}