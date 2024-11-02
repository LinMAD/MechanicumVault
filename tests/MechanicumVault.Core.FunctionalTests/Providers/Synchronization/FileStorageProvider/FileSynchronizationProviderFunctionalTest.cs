using MechanicumVault.Core.Providers.Synchronization;
using MechanicumVault.Core.Providers.Synchronization.FileStorageProvider;
using MechanicumVault.Core.Providers.Synchronization.FileStorageProvider.Factories;
using Xunit;

namespace MechanicumVault.Core.FunctionalTests.Providers.Synchronization.FileStorageProvider;

public class FileSynchronizationProviderFunctionalTest
{
	private const string TestFileName = "mechanicum_vault_unit_test.txt";

	[Fact]
	public void FileSynchronizationProvider_ObserverEvents()
	{
		string testDirectory = Path.Combine(
			Path.GetTempPath(),
			"MechanicumVaultFileSynchronizationProviderFunctionalTest"
		);
		string testFilePath = Path.Combine(testDirectory, TestFileName);
		Directory.CreateDirectory(testDirectory);

		var provider = new FileSynchronizationProvider(new FileSystemWatcherFactory());
		provider.SetNewObservablePointOfInterest(testDirectory);

		provider.OnFileChanged += FileSynchronizationProviderOnFileChanged;
		provider.Observe();

		File.Create(testFilePath).Close();
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		File.WriteAllText(testFilePath, "Unit Test");
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		;
		File.AppendAllText(testFilePath, "Answer is 42");
		Thread.Sleep(TimeSpan.FromMilliseconds(100));
		File.Delete(testFilePath);
		Thread.Sleep(TimeSpan.FromMilliseconds(100));

		Directory.Delete(testDirectory, true); // Clean up
	}

	private void FileSynchronizationProviderOnFileChanged(object? sender, SynchronizationEvent e)
	{
		Assert.NotNull(sender);

		switch (e.ChangeType)
		{
			case SynchronizationChangeType.Created:
				Assert.True(File.Exists(e.FilePath) && Path.GetFileName(e.FilePath) == TestFileName);
				break;
			case SynchronizationChangeType.Changed:
				Assert.True(File.Exists(e.FilePath) && Path.GetFileName(e.FilePath) == TestFileName);
				break;
			case SynchronizationChangeType.Deleted:
				Assert.False(File.Exists(e.FilePath));
				Assert.True(Path.GetFileName(e.FilePath) == TestFileName);
				break;
			case SynchronizationChangeType.Renamed:
				Assert.True(File.Exists(e.FilePath) && Path.GetFileName(e.FilePath) == TestFileName);
				break;
			default:
				throw new ArgumentOutOfRangeException(); // Must be never triggered
		}
	}
}