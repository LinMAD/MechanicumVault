using MechanicumVault.Core.Exceptions;
using MechanicumVault.Core.Infrastructure.Providers.Synchronization.FileStorageProvider.Factories;

namespace MechanicumVault.Core.Infrastructure.Providers.Synchronization.FileStorageProvider;

/// <summary>
/// Local file synchronization provided for monitoring changes.
/// </summary>
public class FileSynchronizationProvider : ISynchronizationProvider
{
	private readonly IFileSystemWatcherFactory _fileSystemWatcherFactory;
	private FileSystemWatcher? _fileSystemWatcher;

	public FileSynchronizationProvider(IFileSystemWatcherFactory fileSystemWatcherFactory)
	{
		_fileSystemWatcherFactory = fileSystemWatcherFactory;
	}

	public void Observe()
	{
		if (_fileSystemWatcher == null)
		{
			throw new InvalidConfiguration(
				"Point of interest (source directory) must be configured before observing it."
			);
		}

		_fileSystemWatcher.EnableRaisingEvents = true;
		_fileSystemWatcher.Created += OnChanged;
		_fileSystemWatcher.Deleted += OnChanged;
		_fileSystemWatcher.Changed += OnChanged;
		_fileSystemWatcher.Renamed += OnRenamed;
	}

	/// <summary>
	/// Creates new File watcher in given pointOfInterest like source directory.
	/// </summary>
	/// <param name="pointOfInterest">Directory</param>
	/// <exception cref="NotFoundException">If directory not exist</exception>
	/// <exception cref="MechanicumVaultException">Unexpected exception</exception>
	public void SetNewObservablePointOfInterest(string pointOfInterest)
	{
		if (Directory.Exists(pointOfInterest) is not true)
		{
			throw new NotFoundException($"Directory {pointOfInterest} does not exist");
		}

		try
		{
			_fileSystemWatcher = _fileSystemWatcherFactory.Create(pointOfInterest);
			_fileSystemWatcher.IncludeSubdirectories = true;
		}
		catch (Exception e)
		{
			throw new MechanicumVaultException(
				$"Unable to observe point of interest({pointOfInterest}) filsystem watcher error: {e.Message}"
			);
		}
	}

	#region Event declaration

	public event EventHandler<SynchronizationEvent> OnFileChanged = null!;

	private void OnChanged(object sender, FileSystemEventArgs e)
	{
		OnFileChanged?.Invoke(
			this,
			new SynchronizationEvent
			{
				FilePath = e.FullPath,
				ChangeType = (SynchronizationChangeType)Enum.Parse(typeof(SynchronizationChangeType), e.ChangeType.ToString())
			}
		);
	}

	private void OnRenamed(object sender, RenamedEventArgs e)
	{
		OnFileChanged?.Invoke(
			this,
			new SynchronizationEvent(filePath: e.FullPath, changeType: SynchronizationChangeType.Renamed));
	}

	#endregion
}