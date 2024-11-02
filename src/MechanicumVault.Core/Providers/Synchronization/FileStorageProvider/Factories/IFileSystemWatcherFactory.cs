namespace MechanicumVault.Core.Providers.Synchronization.FileStorageProvider.Factories;

public interface IFileSystemWatcherFactory
{
	FileSystemWatcher Create(string path);
}