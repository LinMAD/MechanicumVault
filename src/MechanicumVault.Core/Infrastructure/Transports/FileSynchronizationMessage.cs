using System.Text;
using System.Text.Json;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Providers.Synchronization;

namespace MechanicumVault.Core.Infrastructure.Transports;

/// <summary>
/// FileChangeMessage is a standard object for transports communication.
/// In case when adapter will send a message and port will receive it. 
/// </summary>
public record FileSynchronizationMessage(SynchronizationChangeType SyncChangeType, string FilePath)
{
	public byte[] ToBytes()
	{
		return FilePath == string.Empty ? [] : JsonSerializer.SerializeToUtf8Bytes(this, JsonSerializerOptions);
	}

	public static FileSynchronizationMessage? FromBytes(byte[] data, int count)
	{
		if (data == null || data.Length == 0 || count <= 0)
		{
			throw new RuntimeException($"FileSynchronizationMessage {nameof(data)} must not be null or empty.");
		}

		try
		{
			var jsonSpan = new ReadOnlySpan<byte>(data, 0, count);
			return JsonSerializer.Deserialize<FileSynchronizationMessage>(jsonSpan, JsonSerializerOptions);
		}
		catch (JsonException ex)
		{
			throw new DeserializationException("Error deserializing FileSynchronizationMessage from JSON.", ex);
		}
	}
	
	private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};
}