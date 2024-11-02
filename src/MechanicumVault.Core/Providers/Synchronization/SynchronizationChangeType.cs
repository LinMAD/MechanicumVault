namespace MechanicumVault.Core.Providers.Synchronization;

/// <summary>
/// SynchronizationChangeType representing the type of change that occurred.
/// </summary>
public enum SynchronizationChangeType
{
	Created,
	Changed,
	Deleted,
	Renamed
}