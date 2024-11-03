using System.Text;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Providers.Synchronization;
using MechanicumVault.Core.Infrastructure.Transports;
using Xunit;

namespace MechanicumVault.Core.UnitTests.Infrastructure.Transports;

public class FileSynchronizationMessageUnitTest
{
	[Fact]
	public void FileSynchronizationMessage_ShouldSerializeAndDeserializeCorrectly()
	{
		var expectedMessage = new SynchronizationMessage(SynchronizationChangeType.Created, "foo/file.txt");

		var bytes = expectedMessage.ToBytes();
		var deserializedMessage = SynchronizationMessage.FromBytes(bytes, bytes.Length);

		Assert.NotNull(deserializedMessage);
		Assert.Equal(expectedMessage.SyncChangeType, deserializedMessage.SyncChangeType);
		Assert.Equal(expectedMessage.FilePath, deserializedMessage.FilePath);
	}

	[Fact]
	public void FileSynchronizationMessage_BadBytesData()
	{
		byte[] invalidData = [];

		invalidData = Encoding.UTF8.GetBytes("");
		Assert.Throws<DeserializationException>(() => SynchronizationMessage.FromBytes(invalidData, invalidData.Length));

		invalidData = new byte[1024];
		Assert.Throws<DeserializationException>(() => SynchronizationMessage.FromBytes(invalidData, invalidData.Length));
	}

}