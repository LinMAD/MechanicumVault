namespace MechanicumVault.Core.Infrastructure.Providers.Synchronization.FileStorageProvider.Factories;

public interface IFileSystemWatcherFactory
{
	FileSystemWatcher Create(string path);
}