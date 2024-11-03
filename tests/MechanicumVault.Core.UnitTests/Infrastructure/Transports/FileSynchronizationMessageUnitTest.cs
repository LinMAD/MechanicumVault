using System.Text;
using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Transports;
using MechanicumVault.Core.Providers.Synchronization;
using Xunit;

namespace MechanicumVault.Core.UnitTests.Infrastructure.Transports;

public class FileSynchronizationMessageUnitTest
{
	[Fact]
	public void FileSynchronizationMessage_ShouldSerializeAndDeserializeCorrectly()
	{
		var expectedMessage = new FileSynchronizationMessage(SynchronizationChangeType.Created, "foo/file.txt");

		var bytes = expectedMessage.ToBytes();
		var deserializedMessage = FileSynchronizationMessage.FromBytes(bytes, bytes.Length);

		Assert.NotNull(deserializedMessage);
		Assert.Equal(expectedMessage.SyncChangeType, deserializedMessage.SyncChangeType);
		Assert.Equal(expectedMessage.FilePath, deserializedMessage.FilePath);
	}

	[Fact]
	public void FileSynchronizationMessage_BadBytesData()
	{
		var invalidData = Encoding.UTF8.GetBytes("Invalid JSON string");
		
		Assert.Throws<DeserializationException>(() => FileSynchronizationMessage.FromBytes(invalidData, invalidData.Length));
	}
	
}