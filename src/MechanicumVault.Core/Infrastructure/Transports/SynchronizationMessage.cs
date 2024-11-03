using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Providers.Synchronization;
using MessagePack;

namespace MechanicumVault.Core.Infrastructure.Transports;

/// <summary>
/// SynchronizationMessage is a standard object for transports communication.
/// In case when adapter will send a message and port will receive it. 
/// </summary>
[MessagePackObject]
public record SynchronizationMessage
{
	[Key(0)]
	public SynchronizationChangeType SyncChangeType { get; init; } = SynchronizationChangeType.Uknown;

	[Key(1)]
	public string FilePath { get; init; } = string.Empty;

	public SynchronizationMessage(SynchronizationChangeType syncChangeType, string filePath)
	{
		SyncChangeType = syncChangeType;
		FilePath = filePath;
	}

	public byte[] ToBytes()
	{
		return FilePath == string.Empty ? [] : MessagePackSerializer.Serialize(this);
	}

	public static SynchronizationMessage? FromBytes(byte[] data, int count)
	{
		if (data == null || data.Length == 0 || count <= 0)
		{
			throw new DeserializationException($"FileSynchronizationMessage {nameof(data)} must not be null or empty.");
		}

		try
		{
			return MessagePackSerializer.Deserialize<SynchronizationMessage>(data);
		}
		catch (MessagePackSerializationException ex)
		{
			throw new DeserializationException("Error deserializing FileSynchronizationMessage from JSON.", ex);
		}
	}
}