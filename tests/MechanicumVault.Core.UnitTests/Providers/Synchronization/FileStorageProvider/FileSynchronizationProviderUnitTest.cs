using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Providers.Synchronization.FileStorageProvider;
using MechanicumVault.Core.Providers.Synchronization.FileStorageProvider.Factories;
using Moq;
using Xunit;

namespace MechanicumVault.Core.UnitTests.Providers.Synchronization.FileStorageProvider;

public class FileSynchronizationProviderUnitTest
{
	private readonly Mock<IFileSystemWatcherFactory> _mockFileSystemWatcherFactory;
	private readonly Mock<FileSystemWatcher> _mockFileSystemWatcher;

	public FileSynchronizationProviderUnitTest()
	{
		_mockFileSystemWatcherFactory = new Mock<IFileSystemWatcherFactory>();
		_mockFileSystemWatcher = new Mock<FileSystemWatcher>();
	}

	[Fact]
	public void SetNewObservablePointOfInterest_Success()
	{
		_mockFileSystemWatcherFactory
			.Setup(f => f.Create(It.IsAny<string>()))
			.Returns(_mockFileSystemWatcher.Object);

		var provider = new FileSynchronizationProvider(_mockFileSystemWatcherFactory.Object);

		string testDirectory = Path.GetTempPath();
		Directory.CreateDirectory(testDirectory);

		try
		{
			provider.SetNewObservablePointOfInterest(testDirectory);
		}
		catch (Exception e)
		{
			Assert.Fail(e.Message);
		}
	}

	[Fact]
	public void SetNewObservablePointOfInterest_PointOfInterestNotFound()
	{
		_mockFileSystemWatcherFactory
			.Setup(f => f.Create(It.IsAny<string>()))
			.Returns(_mockFileSystemWatcher.Object);

		var provider = new FileSynchronizationProvider(_mockFileSystemWatcherFactory.Object);

		try
		{
			provider.SetNewObservablePointOfInterest("unit/not-exist-path");
		}
		catch (NotFoundException e)
		{
			Assert.NotEmpty(e.ToString());
		}
		catch (Exception e)
		{
			Assert.Fail(e.Message);
		}
	}

	[Fact]
	public void Observe_BadConfiguration()
	{
		_mockFileSystemWatcherFactory
			.Setup(f => f.Create(It.IsAny<string>()))
			.Returns(_mockFileSystemWatcher.Object);

		var provider = new FileSynchronizationProvider(_mockFileSystemWatcherFactory.Object);

		Assert.Throws<InvalidConfiguration>(() => provider.Observe());
	}

	[Fact]
	public void Observe_OkConfiguration()
	{
		string testDirectory = Path.GetTempPath();
		Directory.CreateDirectory(testDirectory);

		_mockFileSystemWatcherFactory
			.Setup(f => f.Create(testDirectory))
			.Returns(new FileSystemWatcher(testDirectory));

		var provider = new FileSynchronizationProvider(_mockFileSystemWatcherFactory.Object);
		provider.SetNewObservablePointOfInterest(testDirectory);
		provider.Observe();
	}
}