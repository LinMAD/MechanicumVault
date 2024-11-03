namespace MechanicumVault.Core.Infrastructure.Providers.Synchronization.FileStorageProvider.Factories;

public class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
	public FileSystemWatcher Create(string path)
	{
		return new FileSystemWatcher(path);
	}
}