using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using MechanicumVault.App.Server.Common.Configurations;
using MechanicumVault.Core.Configurations;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Transports;
using MechanicumVault.Core.Providers.Synchronization;
using Microsoft.Extensions.Logging;

namespace MechanicumVault.App.Server.Infrastructure.Transports;

/// <summary>
/// TcpTransportPort responsible for listening TcpTransportAdapter that will notify with updates.
/// </summary>
public class TcpTransportPort(ILogger logger, ServerConfiguration serverCfg, ApplicationConfiguration appCfg)
	: ITcpTransport
{
	private TcpListener? _tcpListener;
	private bool _isListening;

	public void Connect()
	{
		try
		{
			_tcpListener = new TcpListener(IPAddress.Any, serverCfg.Port);
			_tcpListener.Start();
			_isListening = true;
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
			logger.LogWarning("Failed to stop TCP listener with error: {msg}", e.Message);
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
	
	public void HandleClientNotification(TcpClient tcpClient)
	{
		// TODO Refactor to several methods or classes => reduce complexity

		try
		{
			// TODO Investigate Case 1: Big giles integrity (hash) maybe client closing to early 
			// TODO Investigate Case 2: Big folders might be not always sync
			// TODO Investigate Case 3: Improve renaming of files must be provided from client => old -> new file / folder
			FileSynchronizationMessage? newFileSynchronizationMessage = null;
			NetworkStream stream = tcpClient.GetStream();
			byte[] buffer = new byte[1024];
			int bytesRead = stream.Read(buffer, 0, buffer.Length);
			
			try
			{
				newFileSynchronizationMessage = FileSynchronizationMessage.FromBytes(buffer, bytesRead);
			}
			catch (DeserializationException e)
			{
				logger.LogError("Client message is corrupted, unable to deserialize bytes with error: {msg}", e.Message);
			}
			
			if (newFileSynchronizationMessage == null) return;

			logger.LogDebug("New message decoded, file: {path}", newFileSynchronizationMessage.FilePath);
			string destinationPath = Path.Combine(appCfg.DestinationDirectory, newFileSynchronizationMessage.FilePath);
			logger.LogDebug("New messages file destination path: {path}", destinationPath);

			switch (newFileSynchronizationMessage.SyncChangeType)
			{
				case SynchronizationChangeType.Created:
				case SynchronizationChangeType.Changed:
				case SynchronizationChangeType.Renamed:
					// Read file length
					byte[] fileLengthBuffer = new byte[4];
					stream.Read(fileLengthBuffer, 0, fileLengthBuffer.Length);
					int fileLength = BitConverter.ToInt32(fileLengthBuffer, 0);

					// Read file data
					byte[] fileData = new byte[fileLength];
					int totalBytesRead = 0;
					while (totalBytesRead < fileLength)
					{
						int bytesToRead = Math.Min(fileLength - totalBytesRead, buffer.Length);
						int bytesReadInIteration = stream.Read(fileData, totalBytesRead, bytesToRead);
						if (bytesReadInIteration == 0)
						{
							throw new RuntimeException("Unexpected end of stream while reading file data");
						}

						totalBytesRead += bytesReadInIteration;
					}

					// Read hash for integrity check
					byte[] hashBuffer = new byte[32];
					stream.Read(hashBuffer, 0, hashBuffer.Length);

					// TODO Investigate why computed has can be 0,0,0...
					byte[] computedHash = SHA256.HashData(fileData);
					if (!IsHashSame(computedHash, hashBuffer))
					{
						logger.LogWarning(
							"File might be corrupted, integrity check failed for {path}, hash: {hash}, computed hash: {computedHash}",
							destinationPath,
							hashBuffer,
							computedHash
						);
						break;
					}

					if (Path.GetDirectoryName(destinationPath) == string.Empty ||
					    Path.GetDirectoryName(destinationPath) == null)
					{
						logger.LogError(
							"On the server side directory is empty, file {path} will be not processed for synchronization.",
							destinationPath
						);
						break;
					}

					Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
					File.WriteAllBytes(destinationPath, fileData);

					logger.LogInformation(
						"Synchronization Event {type} is done for file: {path}",
						newFileSynchronizationMessage.SyncChangeType,
						destinationPath
					);
					break;
				case SynchronizationChangeType.Deleted:
					// TODO Not accurate => client must provide context if it was folder or file that can have same name as folder
					if (File.Exists(destinationPath))
					{
						File.Delete(destinationPath);
					} else if (Directory.Exists(destinationPath))
					{
						Directory.Delete(destinationPath, true);
					}
					
					logger.LogInformation(
						"Synchronization Event {type} is done for file: {path}",
						newFileSynchronizationMessage.SyncChangeType,
						destinationPath
					);
					break;
				case SynchronizationChangeType.Uknown:
				default:
					throw new NotFoundException("Unhandled synchronization change type");
			}
		}
		catch (Exception e)
		{
			logger.LogWarning("Error while handling client: {msg}", e.Message);
		}
		finally
		{
			if (_tcpListener != null)
			{
				tcpClient.Close();				
			}
		}
	}

	public bool IsAwaitingClients()
	{
		return _isListening;
	}

	private bool IsHashSame(byte[] hash1, byte[] hash2)
	{
		if (hash1.Length != hash2.Length) return false;

		return !hash1.Where((t, i) => t != hash2[i]).Any();
	}
}