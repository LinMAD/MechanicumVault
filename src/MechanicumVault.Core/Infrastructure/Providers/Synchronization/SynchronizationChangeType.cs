namespace MechanicumVault.Core.Infrastructure.Providers.Synchronization;

/// <summary>
/// SynchronizationChangeType representing the type of change that occurred.
/// </summary>
public enum SynchronizationChangeType
{
	Uknown,
	Created,
	Changed,
	Deleted,
	Renamed
}