namespace MechanicumVault.Core.Providers.Synchronization;

/// <summary>
/// SynchronizationEvent that is used for implementation of ISynchronizationProvider.
///
/// Event arg are used to provide details about a file or folder change.
/// </summary>
public class SynchronizationEvent
{
	/// <summary>
	/// File path of the file that was changed.
	/// </summary>
	public string FilePath { get; set; }

	/// <summary>
	/// Type of change that occurred.
	/// </summary>
	public SynchronizationChangeType ChangeType { get; set; }

	public SynchronizationEvent()
	{
		FilePath = string.Empty;
	}

	public SynchronizationEvent(string filePath, SynchronizationChangeType changeType)
	{
		FilePath = filePath;
		ChangeType = changeType;
	}
}