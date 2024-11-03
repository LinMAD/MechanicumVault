namespace MechanicumVault.Core.Infrastructure.Providers.Synchronization;

/// <summary>
/// Common interface to provide monitoring file changes and notifying listeners of such changes.
/// </summary>
public interface ISynchronizationProvider
{
	/// <summary>
	/// Must observe Point Of Interest and produce SynchronizationEvent's.
	/// </summary>
	void Observe();

	/// <summary>
	/// Sets point of interest like folder that must be monitored for changes.
	/// </summary>
	/// <returns>bool if point of interest can be observed</returns>
	void SetNewObservablePointOfInterest(string pointOfInterest);

	/// <summary>
	/// Event that is triggered on file changes.
	/// </summary>
	event EventHandler<SynchronizationEvent> OnFileChanged;
}